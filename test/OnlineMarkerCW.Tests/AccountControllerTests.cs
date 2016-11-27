using System;
using Xunit;
using Xunit.Abstractions;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using OnlineMarkerCW.Models;
using OnlineMarkerCW.ViewModels;
using OnlineMarkerCW.Controllers;
using OnlineMarkerCW.Interfaces;
using OnlineMarkerCW.UnitTests.ObjectPrintHelper;

namespace OnlineMarkerCW.UnitTests.Controllers
{
    public class AccountController_UnitTests
    {
        private readonly AccountController _accountController;
        private readonly LoggerFactory _logger;
        private readonly ITestOutputHelper output;
        private ObjectPrinter objectPrinter;
        private Mock<UserManager<ApplicationUser>> m_userManager;
        private UserManager<ApplicationUser> userManager;
        private Mock<SignInManager<ApplicationUser>> m_signInManager;
        private SignInManager<ApplicationUser> signInManager;
        private Mock<RoleManager<ApplicationUserRole>> m_roleManager;
        private RoleManager<ApplicationUserRole> roleManager;
        private Mock<IDbServices> m_dbServices;

        public AccountController_UnitTests(ITestOutputHelper output)
         {
             this.objectPrinter                 = new ObjectPrinter (output);
             this.output                        = output;
              _logger                           = new LoggerFactory();
              var m_actionContext               = new ActionContext();
              var m_IUserStore                  = new Mock<IUserStore<ApplicationUser>>();
              //var userStore                     = new UserStore<ApplicationUser>();
              this.m_userManager                = new Mock<UserManager<ApplicationUser>>(m_IUserStore.Object, null, null, null, null, null, null, null, null);
              this.userManager                  = new UserManager<ApplicationUser>(m_IUserStore.Object, null, null, null, null, null, null, null, null);
              var m_IHttpContextAccessor        = new Mock<IHttpContextAccessor>();
              var m_IUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
              this.m_signInManager              = new Mock<SignInManager<ApplicationUser>>(m_userManager.Object, m_IHttpContextAccessor.Object, m_IUserClaimsPrincipalFactory.Object, null, null);
              this.signInManager                = new SignInManager<ApplicationUser>(userManager, m_IHttpContextAccessor.Object, m_IUserClaimsPrincipalFactory.Object, null, null);
              var m_IRoleStore                  = new Mock<IRoleStore<ApplicationUserRole>>();
              this.m_roleManager                = new Mock<RoleManager<ApplicationUserRole>>(m_IRoleStore.Object, null, null, null, null, null);
              this.roleManager                  = new RoleManager<ApplicationUserRole>(m_IRoleStore.Object, null, null, null, null, null);
              this.m_dbServices                 = new Mock<IDbServices>();

              _accountController = new AccountController  (
                  //use real logger
                  loggerFactory : _logger, //loggins it not going to work, use xUnit's output.Write
                  //Inject mock dependencies
                  userManager   : m_userManager.Object,
                  signInManager : m_signInManager.Object,
                  roleManager   : m_roleManager.Object,
                  dbServices    : m_dbServices.Object
                  //accountconroller needs Url to be set as well, otherwise when it is called in the controller it is going to throw a null exception.
             ) {  Url = new UrlHelper(m_actionContext) };
         }

         [Fact]
         public void  GET_Login_IndexReturnsLoginView_WhenUserNotLoggedIN() {
            //Arrange
            //Act
            var result = _accountController.Login();
            //Assert that view is returned and that name of it os Login.
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(viewResult.ViewName, "Login");
         }

         [Fact]
         public async Task   POST_Login_IndexReturnsLoginView_WhenWrongCredentials() {
            //Arrange
            var m_loginViewModel = new LoginViewModel() {Email = "idontexist@anyhwere.com", Password = "cannotpossibliygowrongwiththisone1111" };
            var m_signInResult   = Microsoft.AspNetCore.Identity.SignInResult.Failed; //	Returns a SignInResult that represents a failed sign-in.
            m_signInManager.Setup(s => s.PasswordSignInAsync(m_loginViewModel.Email, m_loginViewModel.Password, false, false)).Returns(Task.FromResult(m_signInResult)); //async methods require Task object to be returned.
            //Act
            var result = await _accountController.Login(m_loginViewModel);
            output.WriteLine("#################################");
            output.WriteLine("Printing the result");
            output.WriteLine("#################################");
            objectPrinter.printObject(result);
            output.WriteLine("#################################");
            output.WriteLine("Printing the Modelstate");
            output.WriteLine("#################################");
            objectPrinter.printObject(_accountController.ModelState.Values);
            //Assert that view is returned, that name of it is Login and that ModelSate containts ErrorMessage neccesarry
            var viewResult = Assert.IsType<ViewResult>(result);
                //That is the way to access ModelErrorCollection from the modelSate
            var errorResulst = (_accountController.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == "Invalid login attempt.").Count() == 0) ? false : true;
            Assert.NotNull(viewResult);
            Assert.Equal(viewResult.ViewName, "Login");
            Assert.True(errorResulst);
         }

         [Fact]
         public async Task POST_Login_IndexReturnHomeIndex_WhenRightCredentianslandNoRedirectUrl() {
           //Arrange
           var m_loginViewModel = new LoginViewModel() {Email = "idontexist@anyhwere.com", Password = "cannotpossibliygowrongwiththisone1111" };
           var m_signInResult   = Microsoft.AspNetCore.Identity.SignInResult.Success; //	Returns a SignInResult that represents a successful sign-in.
           m_signInManager.Setup(s => s.PasswordSignInAsync(m_loginViewModel.Email, m_loginViewModel.Password, false, false)).Returns(Task.FromResult(m_signInResult)); //async methods require Task object to be returned.
           //Act
           var result = await _accountController.Login(m_loginViewModel);
           output.WriteLine("#################################");
           output.WriteLine("Printing the result");
           output.WriteLine("#################################");
           objectPrinter.printObject(result);
           //Assert that controller redirects to homeIndex
           var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
               //That is the way to access ModelErrorCollection from the modelSate
           Assert.Equal("Home", redirectToActionResult.ControllerName);
           Assert.Equal("Index", redirectToActionResult.ActionName);
         }

    }

}
