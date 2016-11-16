using System;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;

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
         [InlineData(-1)]
         [InlineData(0)]
         [InlineData(1)]
         public void HomeReturnFalseGivenValuesLessThan2(int value)
         {
             var result = _teacherController.IsPrime(value);

             Assert.False(result, $"{value} should not be prime");
         }
    }
}
