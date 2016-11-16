using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;

namespace OnlineMarkerCW.UnitTests.Controllers
{
    public class AccountController_UnitTests
    {
        private readonly OnlineMarkerCW.Controllers.TeacherController _teacherController;

         public AccountController_UnitTests()
         {
             _teacherController = new OnlineMarkerCW.Controllers.TeacherController();
         }

         [Theory]
         [InlineData(-1)]
         [InlineData(0)]
         [InlineData(1)]
         public void AccountReturnFalseGivenValuesLessThan2(int value)
         {
            //test mock creation
            var mockSet = new Mock<DbSet<Work>>();
             var result = _teacherController.IsPrime(value);

             Assert.False(result, $"{value} should not be prime");
         }
    }
}
