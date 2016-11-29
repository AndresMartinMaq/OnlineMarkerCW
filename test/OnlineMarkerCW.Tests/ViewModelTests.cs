using System;
using Xunit;
using Xunit.Abstractions;
using Moq;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
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
using OnlineMarkerCW.ViewModels;
using OnlineMarkerCW.UnitTests.ObjectOperationsHelper;

namespace OnlineMarkerCW.UnitTests.ViewModels
{
    public class ViewModels_UnitTests
    {
      private readonly LoggerFactory _logger;
      private readonly ITestOutputHelper output;
      private ObjectOperations objectOperations;
      public ViewModels_UnitTests(ITestOutputHelper output)
       {
           this._logger                       = new LoggerFactory();
           this.objectOperations              = new ObjectOperations (output);
           this.output                        = output;

      }


      [Theory]
      [InlineData("", "", "The Email field is required.", "The Password field is required.")]
      [InlineData("@test.com", "123", "Invalid Email Address Format.", "The Password must be at least 6 and at max 100 characters long.")]
      [InlineData("test.co", "0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789" +"p", "Invalid Email Address Format.", "The Password must be at least 6 and at max 100 characters long.")]
      public void  LoginViewModel_Password_Or_Email_ShouldBeRightFormat(string email, string password, string emailErrorMsg, string passwordErrorMsg) {
         //Arrange
         var m_loginViewModel = new LoginViewModel() {Email = email, Password = password };
         //Act
         var result = ValidateModel(m_loginViewModel);
         output.WriteLine("#################################");
         output.WriteLine("Printing the result");
         output.WriteLine("#################################");
         objectOperations.printObject(result);
         //Assert that error message for Email is that it is required, same for the Password
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Email").ToList()[0].ToString(), emailErrorMsg);
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Password").ToList()[0].ToString(), passwordErrorMsg);
      }

      [Theory]
      [InlineData("", "", "", "", "", 0,  "The Email field is required.", "The Name field is required.", "The Surname field is required.", "The Password field is required.", "The Confirm Password field is required.", "")]
      [InlineData("@test.com", "1", "1", "123", "123", 2, "Invalid Email Address Format.", "The Name must be at least 2 and at max 100 characters long.", "The Surname must be at least 2 and at max 100 characters long.", "The Password must be at least 6 and at max 100 characters long.", "The Confirm Password must be at least 6 and at max 100 characters long.",  "The type should be either a student or a teacher.")]
      [InlineData("@test.com", "0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789" +"n", "0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789" +"s", "0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789" +"p", "0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789"+"0123456789" +"p", 2, "Invalid Email Address Format.", "The Name must be at least 2 and at max 100 characters long.", "The Surname must be at least 2 and at max 100 characters long.", "The Password must be at least 6 and at max 100 characters long.", "The Confirm Password must be at least 6 and at max 100 characters long.",  "The type should be either a student or a teacher.")]
      public void  RegisterViewModel_Data_ShouldBeRightFormat(string Email,     string     Name,  string      Surname, string      Password, string      ConfirmPassword, int      UserTypeID, string EmailErrorMsg, string   NameErrorMsg, string      SurnameErrorMsg, string      PasswordErrorMsg, string      ConfirmPasswordErrorMsg, string     UserTypeIDErrorMsg) {
         //Arrange
         var m_registerViewModel = new RegisterViewModel() {Email = Email, Name = Name, Surname = Surname, Password = Password, ConfirmPassword = ConfirmPassword, UserTypeID = UserTypeID };
         //Act
         var result = ValidateModel(m_registerViewModel);
         output.WriteLine("#################################");
         output.WriteLine("Printing the result");
         output.WriteLine("#################################");
         objectOperations.printObject(result);
         //Assert that error message is correct for the relevant incorrect data
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Email").ToList()[0].ToString(), EmailErrorMsg);
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Name").ToList()[0].ToString(), NameErrorMsg);
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Surname").ToList()[0].ToString(), SurnameErrorMsg);
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Password").ToList()[0].ToString(), PasswordErrorMsg);
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "ConfirmPassword").ToList()[0].ToString(), ConfirmPasswordErrorMsg);
         if (UserTypeID != 0 && UserTypeID != 1) Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "UserTypeID").ToList()[0].ToString(), UserTypeIDErrorMsg);
      }

      [Theory]
      [InlineData("test@test.com", "name", "surname", "idontmatch1", "idontmatch2", 0)]
      public void  RegisterViewModel_Data_TestCompare(string Email,     string     Name,  string      Surname, string      Password, string      ConfirmPassword, int UserTypeID) {
       //Arrange
       var m_registerViewModel = new RegisterViewModel() {Email = Email, Name = Name, Surname = Surname, Password = Password, ConfirmPassword = ConfirmPassword, UserTypeID = UserTypeID };
       //Act
       var result = ValidateModel(m_registerViewModel);
       output.WriteLine("#################################");
       output.WriteLine("Printing the result");
       output.WriteLine("#################################");
       objectOperations.printObject(result);
       //Assert that error message for non same passwords is correct
       Assert.Equal(result.ToList()[0].ToString(), "The password and confirmation password do not match.");
      }


        //src http://stackoverflow.com/questions/2167811/unit-testing-asp-net-dataannotations-validation
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

    }
}
