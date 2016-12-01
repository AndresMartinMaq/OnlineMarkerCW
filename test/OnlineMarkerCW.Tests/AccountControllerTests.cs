using System;
using Xunit;
using Xunit.Abstractions;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
using OnlineMarkerCW.UnitTests.ObjectOperationsHelper;

namespace OnlineMarkerCW.UnitTests.Controllers
{
    public class AccountController_UnitTests
    {
        private readonly AccountController _accountController;
        private readonly LoggerFactory _logger;
        private readonly ITestOutputHelper output;
        private ObjectOperations objectOperations;
        private Mock<UserManager<ApplicationUser>> m_userManager;
        private UserManager<ApplicationUser> userManager;
        private Mock<SignInManager<ApplicationUser>> m_signInManager;
        private SignInManager<ApplicationUser> signInManager;
        private Mock<RoleManager<ApplicationUserRole>> m_roleManager;
        private RoleManager<ApplicationUserRole> roleManager;
        private Mock<IDbServices> m_dbServices;

        public AccountController_UnitTests(ITestOutputHelper output)
         {
             this.objectOperations                 = new ObjectOperations (output);
             this.output                        = output;
              _logger                           = new LoggerFactory();
              var m_actionContext               = new ActionContext();
              var m_IUserStore                  = new Mock<IUserStore<ApplicationUser>>();
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
         public void  GET_Login_ReturnsLoginView() {
            //Arrange
            //Act
            var result = _accountController.Login();
            //Assert that view is returned and that name of it os Login.
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal(viewResult.ViewName, "Login");
         }

         [Fact]
         //Model Validation happens out of the controller, before action is invoked, hence it cannot be tested in the unit test of the action. Only response to the ModelState.IsValid can be.
         public async Task POST_Login_ReturnsLoginView_WhenModelStateinValid() {
           //Arrange
           var error_mess = "Email is field is Required.";
           var m_loginViewModel = new LoginViewModel() {Email = "", Password = "srthasasdas" };
           _accountController.ModelState.AddModelError(string.Empty, error_mess);
           //Act
            var result = await _accountController.Login(m_loginViewModel);
            output.WriteLine("#################################");
            output.WriteLine("Printing the result");
            output.WriteLine("#################################");
            objectOperations.printObject(_accountController.ModelState);
           //Assert that login view is returned back. Assert that ModelState contains errors. Asssert That validation State is percieved as invalid.
           var viewResult = Assert.IsType<ViewResult>(result);
           var errorResulst = (_accountController.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == error_mess).Count() == 0) ? false : true;

           Assert.NotNull(viewResult);
           Assert.Equal(viewResult.ViewName, "Login");
           Assert.True(errorResulst);
           Assert.Equal(_accountController.ModelState.GetValidationState(""), ModelValidationState.Invalid);
         }

         [Fact]
         public async Task POST_Login_ReturnsLoginView_WhenWrongCredentials() {
            //Arrange
            var m_loginViewModel = new LoginViewModel() {Email = "idontexist@anyhwere.com", Password = "cannotpossibliygowrongwiththisone1111" };
            var m_signInResult   = Microsoft.AspNetCore.Identity.SignInResult.Failed; //	Returns a SignInResult that represents a failed sign-in.
            m_signInManager.Setup(s => s.PasswordSignInAsync(m_loginViewModel.Email, m_loginViewModel.Password, false, false)).Returns(Task.FromResult(m_signInResult)); //async methods require Task object to be returned.
            //Act
            var result = await _accountController.Login(m_loginViewModel);
            output.WriteLine("#################################");
            output.WriteLine("Printing the result");
            output.WriteLine("#################################");
            objectOperations.printObject(result);
            output.WriteLine("#################################");
            output.WriteLine("Printing the Modelstate");
            output.WriteLine("#################################");
            objectOperations.printObject(_accountController.ModelState.Values);
            //Assert that view is returned, that name of it is Login and that ModelSate containts ErrorMessage neccesarry
            var viewResult = Assert.IsType<ViewResult>(result);
                //That is the way to access ModelErrorCollection from the modelSate
            var errorResulst = (_accountController.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == "Invalid login attempt.").Count() == 0) ? false : true;
            Assert.NotNull(viewResult);
            Assert.Equal(viewResult.ViewName, "Login");
            Assert.True(errorResulst);
            Assert.Equal(_accountController.ModelState.GetValidationState(""), ModelValidationState.Invalid);
         }

         [Fact]
         public async Task POST_Login_ReturnHomeIndex_WhenRightCredentialnsandNoReturnUrl() {
           //Arrange
           var m_loginViewModel = new LoginViewModel() {Email = "idontexist@anyhwere.com", Password = "cannotpossibliygowrongwiththisone1111" };
           var m_signInResult   = Microsoft.AspNetCore.Identity.SignInResult.Success; //	Returns a SignInResult that represents a successful sign-in.
           //m_signInManager.Setup(s => s.PasswordSignInAsync(m_loginViewModel.Email, m_loginViewModel.Password, false, false)).Returns(Task.FromResult(m_signInResult)); //async methods require Task object to be returned.
           m_signInManager.Setup(s => s.PasswordSignInAsync(m_loginViewModel.Email, m_loginViewModel.Password, false, false)).ReturnsAsync(m_signInResult);
           //Act
           var result = await _accountController.Login(m_loginViewModel);
           output.WriteLine("#################################");
           output.WriteLine("Printing the result");
           output.WriteLine("#################################");
           objectOperations.printObject(result);
           //Assert that controller redirects to homeIndex
           var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
           Assert.Equal("Home", redirectToActionResult.ControllerName);
           Assert.Equal("Index", redirectToActionResult.ActionName);
         }

         [Fact]
         public async Task POST_Login_ReturnsRedirect_WhenRightCredentianls_withReturnURL() {
           //Arrange
           var returnUrl  = "/Home/About";
           var m_loginViewModel = new LoginViewModel() {Email = "idontexist@anyhwere.com", Password = "cannotpossibliygowrongwiththisone1111" };
           var m_signInResult   = Microsoft.AspNetCore.Identity.SignInResult.Success; //	Returns a SignInResult that represents a successful sign-in.
           m_signInManager.Setup(s => s.PasswordSignInAsync(m_loginViewModel.Email, m_loginViewModel.Password, false, false)).ReturnsAsync(m_signInResult);
           //Act
           var result = await _accountController.Login(m_loginViewModel, returnUrl);
           output.WriteLine("#################################");
           output.WriteLine("Printing the result");
           output.WriteLine("#################################");
           objectOperations.printObject(result);
           //Assert that redirect URL is the same as passed in.
           var redirectResult = Assert.IsType<RedirectResult>(result);
           Assert.Equal(returnUrl, redirectResult.Url);
         }

         [Fact]
         public void  GET_Register_ReturnsLoginVieW() {
            //Arrange
            //Act
            var result = _accountController.Register();
            //Assert that view is returned and that name of it os Register.
            var viewResult = Assert.IsType<ViewResult>(result);
            RegisterViewModel viewModel = (RegisterViewModel)viewResult.Model;
            output.WriteLine("#################################");
            output.WriteLine("Printing the ViewResult");
            output.WriteLine("#################################");
            objectOperations.printObject(viewModel);
            //Assert that register view is returned and that they containted genered student and teacher options.
            Assert.NotNull(viewResult);
            Assert.Equal(viewResult.ViewName, "Register");
            Assert.NotNull(viewModel.UserTypeList.Where(l => l.Text == "Student"));
            Assert.NotNull(viewModel.UserTypeList.Where(l => l.Text == "Teacher"));
         }

         [Fact]
         public async Task POST_Register_ReturnsLoginView_WhenModelStateinValid() {
           //Arrange
           var error_mess = "Name is field is Required.";
           var m_registerViewModel = new RegisterViewModel() {  Email = "test@test.com",   Name  = "TestPersonName",   Surname = "TestPersonSurname",   Password = "testPassword1234",     ConfirmPassword  = "testPassword1234",    UserTypeID = 0};

           _accountController.ModelState.AddModelError(string.Empty, error_mess);
           //Act
            var result = await _accountController.Register(m_registerViewModel);
            output.WriteLine("#################################");
            output.WriteLine("Printing the result");
            output.WriteLine("#################################");
            objectOperations.printObject(_accountController.ModelState);
           //Assert that Register view is returned back. Assert that ModelState contains errors. Asssert That validation State is percieved as invalid.
           var viewResult = Assert.IsType<ViewResult>(result);
           var errorResulst = (_accountController.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == error_mess).Count() == 0) ? false : true;

           Assert.NotNull(viewResult);
           Assert.Equal(viewResult.ViewName, "Register");
           Assert.True(errorResulst);
           Assert.Equal(_accountController.ModelState.GetValidationState(""), ModelValidationState.Invalid);
         }

         [Fact]
         public async Task POST_Register_ReturnsLoginView_WhenRegisterFails() {
           //Arrange
           var m_registerViewModel = new RegisterViewModel() { Email = "test@test.com",   Name  = "TestPersonName",   Surname = "TestPersonSurname",   Password = "testPassword1234",     ConfirmPassword  = "testPassword1234",    UserTypeID = 0};
           var user = new ApplicationUser() {UserName = m_registerViewModel.Email, Email = m_registerViewModel.Email, Name = m_registerViewModel.Name, Surname = m_registerViewModel.Surname};
           var error_msg = "Registration Has Failed";
           var m_identityError = new IdentityError() {Code = "1", Description = error_msg};
           var m_RegisterResult   =  Microsoft.AspNetCore.Identity.IdentityResult.Failed(m_identityError); //	Returns a m_RegisterResult that represents a failed registration.
           m_userManager.Setup(u => u.CreateAsync(It.Is<ApplicationUser>(i => i.UserName == user.UserName), m_registerViewModel.Password)).ReturnsAsync(m_RegisterResult);//cannot mock for an objcet argument for CreateAsync, need to compared property be proeprty of the object.
           //m_userManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(m_RegisterResult);
           //Act
            var result = await _accountController.Register(m_registerViewModel);
            output.WriteLine("#################################");
            output.WriteLine("Printing the result");
            output.WriteLine("#################################");
            objectOperations.printObject(result);
            objectOperations.printObject(_accountController.ModelState);
           //Assert that Register view is returned back. Assert that ModelState contains errors. Asssert That validation State is percieved as invalid.
           var viewResult = Assert.IsType<ViewResult>(result);
           var errorResulst = (_accountController.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == error_msg).Count() == 0) ? false : true;
           Assert.NotNull(viewResult);
           Assert.Equal(viewResult.ViewName, "Register");
           Assert.True(errorResulst);
           Assert.Equal(_accountController.ModelState.GetValidationState(""), ModelValidationState.Invalid);
         }

         [Fact]
         public async Task POST_Register_ReturnsLoginView_WhenRoleCreationFails() {
           //Arrange
           var error_msg = "Role Creation Has Failed";
           var userType = "Student";
           var m_registerViewModel = new RegisterViewModel() { Email = "test@test.com",   Name  = "TestPersonName",   Surname = "TestPersonSurname",   Password = "testPassword1234",     ConfirmPassword  = "testPassword1234",    UserTypeID = 0};
           var user = new ApplicationUser() {UserName = m_registerViewModel.Email, Email = m_registerViewModel.Email, Name = m_registerViewModel.Name, Surname = m_registerViewModel.Surname};
           var role = new ApplicationUserRole() { Name = userType, Description = userType};
           var m_RegisterResult   =  Microsoft.AspNetCore.Identity.IdentityResult.Success; //	Returns a m_RegisterResult that represents a sucesfull registration.
           var m_identityError = new IdentityError() {Code = "1", Description = error_msg};
           var m_roleCreateResult   =  Microsoft.AspNetCore.Identity.IdentityResult.Failed(m_identityError); //Returns an error for role create result
           m_userManager.Setup(u => u.CreateAsync(It.Is<ApplicationUser>(i => i.UserName == user.UserName), m_registerViewModel.Password)).ReturnsAsync(m_RegisterResult);
           m_roleManager.Setup(r => r.RoleExistsAsync(userType)).ReturnsAsync(false);
           m_roleManager.Setup(r => r.CreateAsync(It.Is<ApplicationUserRole>(i => i.Name == role.Name))).ReturnsAsync(m_roleCreateResult);
           //Act
            var result = await _accountController.Register(m_registerViewModel);
            output.WriteLine("#################################");
            output.WriteLine("Printing the result");
            output.WriteLine("#################################");
            objectOperations.printObject(result);
            objectOperations.printObject(_accountController.ModelState);
           //Assert that Register view is returned back. Assert that ModelState contains errors. Asssert That validation State is percieved as invalid.
           var viewResult = Assert.IsType<ViewResult>(result);
           var errorResulst = (_accountController.ModelState.Values.ToList()[0].Errors.Where(e => e.ErrorMessage == error_msg).Count() == 0) ? false : true;
           Assert.NotNull(viewResult);
           Assert.Equal(viewResult.ViewName, "Register");
           Assert.True(errorResulst);
           Assert.Equal(_accountController.ModelState.GetValidationState(""), ModelValidationState.Invalid);
         }

         [Theory]
         [InlineData(true)]
         [InlineData(false)]
         public async Task POST_Register_RedirectHomeIndex_WhenRegiserSucceeds(bool roleExists) {
           //Arrange
           var userType = "Student";
           var m_registerViewModel = new RegisterViewModel() { Email = "test@test.com",   Name  = "TestPersonName",   Surname = "TestPersonSurname",   Password = "testPassword1234",     ConfirmPassword  = "testPassword1234",    UserTypeID = 0};
           var user = new ApplicationUser() {UserName = m_registerViewModel.Email, Email = m_registerViewModel.Email, Name = m_registerViewModel.Name, Surname = m_registerViewModel.Surname};
           var role = new ApplicationUserRole() { Name = userType, Description = userType};
           var m_RegisterResult   =  Microsoft.AspNetCore.Identity.IdentityResult.Success; //	Returns a m_RegisterResult that represents a sucesfull registration.
           var m_roleCreateResult   = Microsoft.AspNetCore.Identity.IdentityResult.Success; // Returns a m_RegisterResult that represents a sucesfull role creation.
           m_userManager.Setup(u => u.CreateAsync(It.Is<ApplicationUser>(i => i.UserName == user.UserName), m_registerViewModel.Password)).ReturnsAsync(m_RegisterResult);
           m_roleManager.Setup(r => r.RoleExistsAsync(userType)).ReturnsAsync(roleExists);
           m_roleManager.Setup(r => r.CreateAsync(It.Is<ApplicationUserRole>(i => i.Name == role.Name))).ReturnsAsync(m_roleCreateResult);
           //Act
          var result = await _accountController.Register(m_registerViewModel);
          output.WriteLine("#################################");
          output.WriteLine("Printing the result");
          output.WriteLine("#################################");
          objectOperations.printObject(result);
          //Assert that controller redirects to homeIndex
          var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
          Assert.Equal("Home", redirectToActionResult.ControllerName);
          Assert.Equal("Index", redirectToActionResult.ActionName);

          //Verify that methods are called
          m_userManager.Verify(u => u.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()),Times.AtLeastOnce());
          m_userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once());
          //cannot mock function which does continat opitonal argumetns
          //m_signInManager.Verify(s => s.SignInAsync(It.IsAny<ApplicationUser>(), false), Times.Once());

         }

         [Fact]
         public async Task  POST_Logout_ReturnsLoginView() {
            //Arrange
            //Act
            var result = await _accountController.Logout();
            //Assert that view is returned and that name of it os Login.
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Account", redirectToActionResult.ControllerName);
            Assert.Equal("Login", redirectToActionResult.ActionName);

            //Verify that methods are called
            m_signInManager.Verify(s => s.SignOutAsync(), Times.Once());
         }

    }

}
