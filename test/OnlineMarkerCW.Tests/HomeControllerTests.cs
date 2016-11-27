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
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;

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
            //-Controller
            var controller = new HomeController(null, new LoggerFactory(), null, null, null);
            //-User
            /*var m_userClaims = new Mock<System.Security.Claims.ClaimsPrincipal>();
            var m_Identity = new Mock<IIdentity>();
            m_Identity.Setup(identity => identity.IsAuthenticated).Returns(true);
            m_userClaims.Setup(u => u.Identity).Returns(m_Identity.Object);
            m_userClaims.Setup(u => u.IsInRole("Student")).Returns(true);*/
            
            //-User Alternative
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.NameIdentifier, "1"),
                 new Claim("Name", "Stuart"),
                 new Claim(ClaimTypes.Surname, "Dent"),
                 new Claim(ClaimTypes.Email, "some@email.com"),
                 new Claim(ClaimTypes.Role, "Student")
            }));

            //m_userClaims.Setup(u => u.FindFirstValue(ClaimTypes.Role)).Returns("Student"); //doesnt work
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


            //Create fake student identity (User).
            //Method 1
            /*GenericIdentity fakeIdentity = new GenericIdentity("SomeUser");
            fakeIdentity.AddClaim(new Claim(ClaimTypes.Role, "Student"));
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);
            //Mock Context to include the fake user
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(t => t.User).Returns(principal);
            var controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(mockHttpContext.Object);*/
            //Set home controller to use mock context
            //controller.ControllerContext = controllerContext.Object;

            /*//Method 2
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "1"),
                 new Claim(ClaimTypes.Role, "Student")
            }));
            //Set context
            controller.ControllerContext = new ControllerContext() {
                HttpContext = new DefaultHttpContext() { User = user }
            };*/


            // Act
            var result = controller.Index();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
            Assert.Equal("MyWorks", redirectToActionResult.ActionName);     //MyWorks if student, MyMarkings if Teacher.
        }
    }
}
