using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;
using System.Threading.Tasks;
using OnlineMarkerCW.Controllers;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;

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
            //test creating of mocks
            var mockSet = new Mock<DbSet<Work>>();
            var result = _teacherController.IsPrime(value);

             Assert.False(result, $"{value} should not be prime");
         }

        //Get a user instance
        private ApplicationUser generateStu() {
            ApplicationUser u = new ApplicationUser();
            u.Name = "Stuart";
            u.Surname = "Studentsen";
            return u;
        }

        [Fact]
        public async Task IndexTest1()
        {
            // --Arrange--
            var controller = new HomeController(null, new LoggerFactory(), null, null, null);
            /*var mockClaims = new Mock<System.Security.Claims.ClaimsPrincipal>();
            mockClaims.Setup(userClaims => userClaims.FindFirstValue(ClaimTypes.Role)).Returns("User");
            controller.User = mockClaims; //Controller.User cannot be set directly as here.*/

            //Create fake student identity (User).
            //Method 1
            GenericIdentity fakeIdentity = new GenericIdentity("SomeUser");
            fakeIdentity.AddClaim(new Claim(ClaimTypes.Role, "Student"));
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);
            //Mock Context to include the fake user
            /*var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(t => t.User).Returns(principal);
            var controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);*/
            //Set home controller to use mock context
            //controller.ControllerContext = controllerContext.Object;

            //Method 2
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "1"),
                 new Claim(ClaimTypes.Role, "Student")
            }));
            //Set context
            controller.ControllerContext = new ControllerContext() {
                HttpContext = new DefaultHttpContext() { User = user }
            };


            // Act
            var result = controller.Index();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
            Assert.Equal("MyWorks", redirectToActionResult.ActionName);     //MyWorks if student, MyMarkings if Teacher.
        }
    }
}

