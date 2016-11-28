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
           this._logger                           = new LoggerFactory();
           this.objectOperations              = new ObjectOperations (output);
           this.output                        = output;
      }


      [Fact]
      public void  LoginViewModel_Password_Or_Email_Cannot_Be_Empty() {
         //Arrange
         var m_loginViewModel = new LoginViewModel() {Email = "", Password = "" };
         //Act
         var result = ValidateModel(m_loginViewModel);
         output.WriteLine("#################################");
         output.WriteLine("Printing the result");
         output.WriteLine("#################################");
         objectOperations.printObject(result);
         //Assert that error message for Email is that it is required, same for the Password
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Email").ToList()[0].ToString(), "The Email field is required.");
         Assert.Equal(result.Where(e => e.MemberNames.ToList()[0] == "Password").ToList()[0].ToString(), "The Password field is required.");
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
