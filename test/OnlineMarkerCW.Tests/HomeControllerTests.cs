using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft​.AspNetCore​.Http​.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;
using System.Threading.Tasks;
using OnlineMarkerCW.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using OnlineMarkerCW.Interfaces;
using OnlineMarkerCW.ViewModels;
using OnlineMarkerCW.UnitTests.ObjectOperationsHelper;
using System.Linq;

namespace OnlineMarkerCW.UnitTests.Controllers
{
    public class HomeController_UnitTests
    {
      private readonly HomeController controller;
      private readonly LoggerFactory _logger;
      private readonly ITestOutputHelper output;
      private ObjectOperations objectOperations;
      private Mock<UserManager<ApplicationUser>> m_userManager;
      private UserManager<ApplicationUser> userManager;
      private Mock<IDbServices> m_dbServices;
      private Mock<IHostingEnvironment> m_hostingEnv;

          public HomeController_UnitTests(ITestOutputHelper output)
           {
               this.objectOperations              = new ObjectOperations (output);
               this.output                        = output;
                _logger                           = new LoggerFactory();
                var m_actionContext               = new ActionContext();
                var m_IUserStore                  = new Mock<IUserStore<ApplicationUser>>();
                //var userStore                     = new UserStore<ApplicationUser>();
                this.m_userManager                = new Mock<UserManager<ApplicationUser>>(m_IUserStore.Object, null, null, null, null, null, null, null, null);
                this.userManager                  = new UserManager<ApplicationUser>(m_IUserStore.Object, null, null, null, null, null, null, null, null);
                this.m_hostingEnv                 = new Mock<IHostingEnvironment>();
                this.m_dbServices                 = new Mock<IDbServices>();

                controller = new HomeController  (
                    //use real logger
                    loggerFactory : _logger, //loggins it not going to work, use xUnit's output.Write
                    //Inject mock dependencies
                    userManager   : m_userManager.Object,
                    hostingEnv    : m_hostingEnv.Object,
                    dbServices    : m_dbServices.Object
               );
           }


        [Theory]
        [InlineData("Student")]
        [InlineData("Teacher")]
        public void GET_Index_RedirectsAccordingToRole(String role)
        {

            // --Arrange--
            var userClaims = getClaims("1", "Stuart", "Dent", "some@email.com", role);
            var contexts = getsContexts(userClaims);
            //Set home controller to use mock context
            controller.ControllerContext = contexts.controllerContext;

            //Force use of ActionExectuing Context.
            controller.OnActionExecuting(contexts.actionExecutingContext);

            // Act
            var result = controller.Index();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
            //Should redirect to MyWorks if student, MyMarkings if Teacher.
            if (role == "Student") {
                Assert.Equal("MyWorks", redirectToActionResult.ActionName);
            } else {
                Assert.Equal("MyMarkings", redirectToActionResult.ActionName);
            }
        }

        [Fact]
        public async Task GET_MyWorks_ReturnViewWithWorksInTheModel() {
            // --Arrange--

            //User
            var userClaims = getClaims("1", "Alice", "Dent", "some@email.com", "Student");
            ApplicationUser user = new ApplicationUser();
            //Works, example list with 3 works.
            List<Work> works = new List<Work>(3);
            for (int i = 1; i < 4; i++) {
                Work w = new Work();
                w.WorkID = i*11;
                w.Owner = user;
                works.Add(w);
            }
            //Controller and database dependencies.
            //--userManager
            m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user);
            //--dbServices
            m_dbServices.Setup(dbs => dbs.GetSubmitedWorks(user)).ReturnsAsync(works);


            //Set home controller to use context with mock user
            var contexts = getsContexts(userClaims);
            controller.ControllerContext = contexts.controllerContext;

            // Act
            ViewResult result = (ViewResult) await controller.MyWorks();

            // Assert
            Assert.Equal(works, result.Model);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GET_WorkView_ReturnsApproiateViewAndModel(int attempt)
        {
            //Arrange
            string Feedback = "Unconstructive feedback";
            Work testWork = new Work();
            testWork.WorkID = 1;
            testWork.Feedback = Feedback;
            testWork.Mark = 10;

            //Setup User for botch cases
              var userClaims = getClaims("5", "Tea", "Cheery", "some@email.com", "Teacher");
              ApplicationUser user = new ApplicationUser();
              testWork.Owner = user;
             if  (attempt == 2){
                userClaims = getClaims("5", "testStundet", "testStundet", "someStudent@email.com", "Student");
                ApplicationUser user2 = new ApplicationUser();
                testWork.Owner = user2;
            }

            //Controller and database dependencies.
            //--userManager
            m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user);
            //--dbServices
            m_dbServices.Setup(dbs => dbs.GetWorkWithID(testWork.WorkID)).ReturnsAsync(testWork);

            //Set home controller to use context with mock user
            var contexts = getsContexts(userClaims);
            controller.ControllerContext = contexts.controllerContext;

            //Act
            var result = await controller.WorkView(testWork.WorkID);

            //Assert
            output.WriteLine("#################################");
            output.WriteLine("Printing the result");
            output.WriteLine("#################################");
            objectOperations.printObject(result);
            if (attempt == 1) {
              var viewResult = Assert.IsType<ViewResult>(result);
              var retrievedWork = (Work)viewResult.Model;
              //Assert that MYWorks viewResult is returned and that model containts the same info
              Assert.NotNull(viewResult);
              Assert.Equal(viewResult.ViewName, "MyWork");
              Assert.Equal(testWork, retrievedWork);
            } else if  (attempt == 2){
              //Assert that wrong user is redirected
              var redirectResult = Assert.IsType<RedirectResult>(result);
              Assert.Equal("/Error_Message/403", redirectResult.Url);
            }
        }

        [Theory]
        [InlineData(60)]    //Marks must be between 0 and 100 to be accepted.
        [InlineData(223)]
        [InlineData(-6)]
        public async Task POST_WorkView_ForMarkerUpdatesWorkWithFeedbackAndMark(int newMark)
        {
            //Arrange
            string newFeedback = "This is useful example feedback";
            string oldFeedback = "Unconstructive feedback";
            Work testWork = new Work();
            testWork.WorkID = 1;
            testWork.Feedback = oldFeedback;
            testWork.Mark = 10;

            //User (Teacher-Marker)
            var userClaims = getClaims("5", "Tea", "Cheery", "some@email.com", "Teacher");
            ApplicationUser user = new ApplicationUser();

            //Controller and database dependencies.
            //--userManager
            m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user);
            //--dbServices
            m_dbServices.Setup(dbs => dbs.GetWorkWithID(testWork.WorkID)).ReturnsAsync(testWork);
            m_dbServices.Setup(dbs => dbs.MarkWork(testWork, user, newFeedback, newMark))
                .Callback((Work w, ApplicationUser u, String fb, int m) =>
                {
                    w.Feedback = fb;
                    w.Mark = m;
                    w.MarkDate = DateTime.Now;
                    w.Marker = u;
                    w.Marked = true;
                });


            //Set home controller to use context with mock user
            var contexts = getsContexts(userClaims);
            controller.ControllerContext = contexts.controllerContext;

            //Act
            ViewResult result = (ViewResult)await controller.WorkViewForMarker(testWork.WorkID, newFeedback, newMark);

            //Assert
            Work retrievedWork = (Work)result.Model;
            if (newMark < 0 || 100 < newMark){
                Assert.Equal(false, controller.ModelState.IsValid);
                Assert.True(controller.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == "Mark should be a number from 0 to 100.").Count() == 1);
                //Assert.Equal(ModelValidationState.Invalid, controller.ModelState.GetValidationState(""));
                Assert.False(retrievedWork.Marked);
                Assert.Equal(oldFeedback, retrievedWork.Feedback);
            } else {
                //Work should have been marked correctly
                Assert.True(retrievedWork.Marked);
                Assert.Equal(newFeedback, retrievedWork.Feedback);
                Assert.Equal(newMark, retrievedWork.Mark);
                Assert.Equal(user, retrievedWork.Marker);
            }
        }

        [Fact]
        public async Task GET_MyWorks_ReturnsRightWorkModelWithView() {
            //Arrange

              //create new student user context
              var userClaims = getClaims("1", "NameTest", "NameTestSurname", "test@email.com", "Student");

              var user1 = new ApplicationUser() {UserName = "test@email.com"};
              var user2 = new ApplicationUser() {UserName = "test2@email.com"};
              //Create new DB entry
              Work testWork = new Work() {WorkID = 1, FilePath="/localpath/path" , Mark = 10, Marked = true, Feedback = "this is feedback", SubmitDate =  DateTime.Now, MarkDate =  DateTime.Now, Owner = user1, Marker = user2};
              var workList = new List<Work> () {testWork};

              //Controller and database dependencies.
              //--userManager
              m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user1);
              //--dbServices
              m_dbServices.Setup(dbs => dbs.GetSubmitedWorks(It.Is<ApplicationUser>(i => i.UserName == user1.UserName))).ReturnsAsync(workList);
              //m_dbServices.Setup(dbs => dbs.GetSubmitedWorks(It.Is<ApplicationUser>(i => i.UserName == user1.UserName))).Returns(Task.FromResult(workList));

              //Set home controller to use context with mock user
              var contexts = getsContexts(userClaims);
              controller.ControllerContext = contexts.controllerContext;

              //Act
              var result = await controller.MyWorks();
              output.WriteLine("#################################");
              output.WriteLine("Printing the result");
              output.WriteLine("#################################");
              objectOperations.printObject(result);
              var viewResult = Assert.IsType<ViewResult>(result);
              var retrievedWork = (List<Work>)viewResult.Model;
              //Assert that MYWorks viewResult is returned and that model containts the same info
              Assert.NotNull(viewResult);
              Assert.Equal(viewResult.ViewName, "MyWorks");
              Assert.Equal(workList, retrievedWork);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
          public async Task POST_MyWorks_ReturnsMyworkViewWithRightModel_ValidationNotPassed(int testAttempt) {

            //Pre-arrange based on the attempt
            var nameIdentifier = "5";
            var error_mess = "File field is required.";
            var m_myWorksViewModel = new MyWorksViewModel() {File = null};
            var getValidationString = "";
            if (testAttempt == 2) {
              //create a Viewmodel with a stream of emtpy file, but with a right extension
              m_myWorksViewModel = getMyWorksViewModel ("", 0, "test.html");

              nameIdentifier = "6";
              error_mess = "Upload failed, please try again, don't forget to check the file size and extension.";
              getValidationString = "File";
            } else if (testAttempt == 3) {
              //create a Viewmodel with a stream of a non emtpy file, but with a wrong extension
              m_myWorksViewModel = getMyWorksViewModel ("<html><body></body></html>", 60, "test.json");

              nameIdentifier = "7";
              error_mess = "Upload failed, please try again, don't forget to check the file size and extension.";
              getValidationString = "File";
            }
            //Arrange

            //Setup hosting env and user id for the path retrieval
            var hosting_path = "./test_wwwroot";
            m_hostingEnv.Setup(h => h.WebRootPath).Returns(hosting_path);
            //create new student user context
            var userClaims = getClaims(nameIdentifier, "NameTest", "NameTestSurname", "test@email.com", "Student");
            var user1 = new ApplicationUser() {UserName = "test@email.com"};
            //Create Work
            Work testWork = new Work() {WorkID = 1, SubmitDate =  DateTime.Now,Owner = user1};
            var workList = new List<Work> () {testWork};

            //Set home controller to use mock context
            var contexts = getsContexts(userClaims);
            controller.ControllerContext = contexts.controllerContext;

            //Force use of ActionExectuing Context.
            controller.OnActionExecuting(contexts.actionExecutingContext);

            //Controller and database dependencies.
            //--userManager
            m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user1);
            //--dbServices
            m_dbServices.Setup(dbs => dbs.GetSubmitedWorks(It.Is<ApplicationUser>(i => i.UserName == user1.UserName))).ReturnsAsync(workList);

            //Add errors to the modelstate
             if (testAttempt == 1) controller.ModelState.AddModelError(string.Empty, error_mess);

            //Act
             var result = await controller.MyWorks(m_myWorksViewModel);
             output.WriteLine("#################################");
             output.WriteLine("Printing the result");
             output.WriteLine("#################################");
             objectOperations.printObject(controller.ModelState);
            //Assert that MyWorks view is returned back. Assert that ModelState contains errors. Asssert That validation State is percieved as invalid.
            var viewResult = Assert.IsType<ViewResult>(result);
            var errorResulst = (controller.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == error_mess).Count() == 0) ? false : true;
            var retrievedWork = (List<Work>)viewResult.Model;
            Assert.NotNull(viewResult);
            Assert.Equal(viewResult.ViewName, "MyWorks");
            Assert.True(errorResulst);
            Assert.Equal(workList, retrievedWork);
            Assert.Equal(controller.ModelState.GetValidationState(getValidationString), ModelValidationState.Invalid);

          }

          [Fact]
          public async Task POST_MyWorks_ReturnsMyworkViewWhenModelAndCreatedFile_WhenValidationPasses() {
                        //Arrange
                        var succes_mess = "File upload sucessfull";
                        //Create workmodel with a file
                        var m_myWorksViewModel = getMyWorksViewModel ("<html><body></body></html>", 60, "test_complete.html");
                        //Setup hosting env and user id for the path retrieval
                        var hosting_path = "./test_wwwroot";
                        m_hostingEnv.Setup(h => h.WebRootPath).Returns(hosting_path);

                        //create new student user context
                        var userClaims = getClaims("8", "NameTest", "NameTestSurname", "test@email.com", "Student");

                        var user1 = new ApplicationUser() {UserName = "test@email.com"};
                        //Create Work
                        Work testWork = new Work() {WorkID = 1, SubmitDate =  DateTime.Now,Owner = user1};
                        Work returnWork = new Work() {};
                        var workList = new List<Work> () {testWork};
                        //Set home controller to use mock context
                        var contexts = getsContexts(userClaims);
                        controller.ControllerContext = contexts.controllerContext;

                        //Force use of ActionExectuing Context.
                        controller.OnActionExecuting(contexts.actionExecutingContext);

                        //Controller and database dependencies.
                        //--userManager
                        m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user1);
                        //--dbServices
                        m_dbServices.Setup(dbs => dbs.GetSubmitedWorks(It.Is<ApplicationUser>(i => i.UserName == user1.UserName))).ReturnsAsync(workList);
                        m_dbServices.Setup(dbs => dbs.AddWork(It.IsAny<Work>()))
                            .Callback((Work w) =>
                            {
                                  returnWork.FileName = w.FileName;
                                  returnWork.FilePath = w.FilePath;
                                  returnWork.SubmitDate = w.SubmitDate;
                                  returnWork.Owner = w.Owner;
                                            });

                        //Act
                         var result = await controller.MyWorks(m_myWorksViewModel);
                          //add the newly generated work to the list
                         workList.Add(returnWork);
                         output.WriteLine("#################################");
                         output.WriteLine("Printing the result");
                         output.WriteLine("#################################");
                         objectOperations.printObject(controller.ModelState);
                         objectOperations.printObject(result);
                        //Assert that MyWorks view is returned back. Assert that ModelState contains errors. Asssert That validation State is percieved as invalid.
                        var viewResult = Assert.IsType<ViewResult>(result);
                        var retrievedWork = (List<Work>)viewResult.Model;
                        Assert.NotNull(viewResult);
                        Assert.Equal(viewResult.ViewName, "MyWorks");
                        Assert.Equal(viewResult.ViewData["upload-message"], succes_mess);
                        Assert.Equal(workList, retrievedWork);
                        //file is created
                        Assert.True(File.Exists(returnWork.FilePath));

          }

          [Theory]
          [InlineData(1)]
          [InlineData(2)]
          public async Task POST_WorkDelete_ToMyWorks_AndDeletesFileForTheUserOwner(int testAttempt) {
                          //Arrange
                          var filepath = Path.GetFullPath("./test_wwwroot/test_delete.html");
                          //Create a new file for testing the delete
                          if (testAttempt == 1) { //Only create fily on the first try as it should stay and asser that, then asser that it should be deleted on the second try
                            using (var fileStream = new FileStream(filepath, FileMode.CreateNew, FileAccess.ReadWrite))
                            {   //create a file
                                var s = "<html><body><p>I am going to be delete without even being noticed by anyone.</p></body></html>";
                                var ms = new MemoryStream();
                                var writer = new StreamWriter(ms);
                                writer.Write(s);
                                writer.Flush();
                                ms.Position = 0;
                                var file = new FormFile(ms, 0, 100, "test_complete.html", "test_complete.html");
                                //save the file stream to the file
                                await file.CopyToAsync(fileStream);
                            }
                          }

                          //create new student user context
                          var userClaims = getClaims("8", "NameTest", "NameTestSurname", "test@email.com", "Student");

                          var user1 = new ApplicationUser() {UserName = "test@email.com"};
                          var user2 = new ApplicationUser() {UserName = "test2@email.com"};
                          //Create Work
                          Work testWork = new Work() {WorkID = 1, FilePath = filepath, SubmitDate =  DateTime.Now, Owner = user2};
                          //Set home controller to use mock context
                          var contexts = getsContexts(userClaims);
                          controller.ControllerContext = contexts.controllerContext;

                          //Force use of ActionExectuing Context.
                          controller.OnActionExecuting(contexts.actionExecutingContext);

                          //Controller and database dependencies.
                          //--userManager
                          if (testAttempt == 1) {
                            m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user1);
                          } else if (testAttempt == 2) {
                            m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user2);
                          }
                          //--dbServices
                          m_dbServices.Setup(dbs => dbs.GetWorkWithID(testWork.WorkID)).ReturnsAsync(testWork);

                          //Act
                           var result = await controller.WorkDelete(testWork.WorkID);
                            //add the newly generated work to the list
                           output.WriteLine("#################################");
                           output.WriteLine("Printing the result");
                           output.WriteLine("#################################");
                           objectOperations.printObject(controller.ModelState);
                           objectOperations.printObject(result);
                          //Assert
                          var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                          //Assert that controller redirects to MyWorks and And Performs delete
                          Assert.Equal("Home", redirectToActionResult.ControllerName);
                          Assert.Equal("MyWorks", redirectToActionResult.ActionName);
                          //file is was deleted
                          if (testAttempt == 1 ) Assert.True(File.Exists(filepath));
                          if (testAttempt == 2 ) Assert.True(!File.Exists(filepath));
            }


            //**Helper methods **//
            //-create a ClaimsPrincipal
            private ClaimsPrincipal getClaims (string iD, string name, string surname, string email, string role)
            {
                return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                     new Claim(ClaimTypes.NameIdentifier, iD),
                     new Claim("Name", name),
                     new Claim(ClaimTypes.Surname, surname),
                     new Claim(ClaimTypes.Email, email),
                     new Claim(ClaimTypes.Role, role)
                }));
           }

           //- create ActionExecutingContext and ControllerContext based on userClaims and return a helper class based on it.
           private TestContexts getsContexts (ClaimsPrincipal UserClaims)
           {
              //Create a HttpContext
              var testHttpCtxt = new DefaultHttpContext() { User = UserClaims };
              //Create ActionExecutingContext.
              var actionContext = new ActionContext(testHttpCtxt, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
              var ListFilterMetaData = new List<IFilterMetadata>() { new Mock<IFilterMetadata>().Object };
              var _actionExecutingContext = new ActionExecutingContext(actionContext, ListFilterMetaData, new Dictionary<string, object>(), null);

              //Set home controller to use mock context
              var _controllerContext = new ControllerContext(new ActionContext(
                  testHttpCtxt,
                  new RouteData(),
                  new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor(),
                  new ModelStateDictionary()));

              return new TestContexts () {actionExecutingContext = _actionExecutingContext, controllerContext = _controllerContext};
            }

            //- a MyWorkVIewModel with a file based on a stream
            private MyWorksViewModel getMyWorksViewModel (string s, int length, string fileName) {
              var ms = new MemoryStream();
              var writer = new StreamWriter(ms);
              writer.Write(s);
              writer.Flush();
              ms.Position = 0;
              var file = new FormFile(ms, 0, length, fileName, fileName);
              return new MyWorksViewModel() {File = file};
            }


    }


    // helper class to contain context information
    public class TestContexts  {
        public ActionExecutingContext actionExecutingContext;
        public ControllerContext controllerContext;
    }
}
