using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
using System.Linq;

namespace OnlineMarkerCW.UnitTests.Controllers
{
    public class HomeController_UnitTests
    {
         private readonly OnlineMarkerCW.Controllers.TeacherController _teacherController;

         public HomeController_UnitTests()
         {
             _teacherController = new OnlineMarkerCW.Controllers.TeacherController();
         }

        [Theory]
        [InlineData("Student")]
        [InlineData("Teacher")]
        public void IndexRedirectsAccordingToRole(String role)
        {
            // --Arrange--
            //-Controller
            var controller = new HomeController(null, new LoggerFactory(), null, null);
            //-User
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.NameIdentifier, "1"),
                 new Claim("Name", "Stuart"),
                 new Claim(ClaimTypes.Surname, "Dent"),
                 new Claim(ClaimTypes.Email, "some@email.com"),
                 new Claim(ClaimTypes.Role, role)
            }));
            
            //Create a HttpContext
            var testHttpCtxt = new DefaultHttpContext() { User = user };
            //Create ActionExecutingContext.
            var actionContext = new ActionContext(testHttpCtxt, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
            var ListFilterMetaData = new List<IFilterMetadata>() { new Mock<IFilterMetadata>().Object };
            var actionExecutingContext = new ActionExecutingContext(actionContext, ListFilterMetaData, new Dictionary<string, object>(), null);

            //Set home controller to use mock context
            var controllerContext = new ControllerContext(new ActionContext(
                testHttpCtxt, 
                new RouteData(), 
                new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor(), 
                new ModelStateDictionary()));
            controller.ControllerContext = controllerContext;

            //Force use of ActionExectuing Context.
            controller.OnActionExecuting(actionExecutingContext);

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

        [Theory]
        [InlineData(60)]    //Marks must be between 0 and 100 to be accepted.
        [InlineData(223)]
        [InlineData(-6)]
        public async Task POST_WorkViewForMarkerUpdatesWorkWithFeedbackAndMark(int newMark)
        {
            //Arrange
            String newFeedback = "This is useful example feedback";
            String oldFeedback = "Unconstructive feedback";
            Work testWork = new Work();
            testWork.WorkID = 1;
            testWork.Feedback = oldFeedback;
            testWork.Mark = 10;

            //User (Teacher-Marker)
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.NameIdentifier, "5"),
                 new Claim("Name", "Tea"),
                 new Claim(ClaimTypes.Surname, "Cheery"),
                 new Claim(ClaimTypes.Email, "some@email.com"),
                 new Claim(ClaimTypes.Role, "Teacher")
            }));
            ApplicationUser user = new ApplicationUser();

            //Create a HttpContext
            var testHttpCtxt = new DefaultHttpContext() { User = userClaims };
            //Create ActionExecutingContext.
            var actionContext = new ActionContext(testHttpCtxt, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
            var ListFilterMetaData = new List<IFilterMetadata>() { new Mock<IFilterMetadata>().Object };
            var actionExecutingContext = new ActionExecutingContext(actionContext, ListFilterMetaData, new Dictionary<string, object>(), null);

            //Controller and database dependencies.
            //--userManager
            var m_IUserStore = new Mock<IUserStore<ApplicationUser>>();
            var m_userManager = new Mock<UserManager<ApplicationUser>>(m_IUserStore.Object, null, null, null, null, null, null, null, null);
            m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user);
            //--dbServices
            var m_dbServices = new Mock<IDbServices>();
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

            //--Controller creation
            var controller = new HomeController(m_userManager.Object, new LoggerFactory(), null, m_dbServices.Object);

            //set to use mock context
            var controllerContext = new ControllerContext(new ActionContext(
                testHttpCtxt,
                new RouteData(),
                new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor(),
                new ModelStateDictionary()));
            controller.ControllerContext = controllerContext;

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

    }
}
