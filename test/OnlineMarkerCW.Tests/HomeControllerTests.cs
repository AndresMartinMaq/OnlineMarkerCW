// ﻿using System;
// using Xunit;
// using Moq;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using OnlineMarkerCW.Models;
// using System.Threading.Tasks;
// using OnlineMarkerCW.Controllers;
// using System.Security.Claims;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Routing;
// using Microsoft.AspNetCore.Mvc.ModelBinding;
// using Microsoft.AspNetCore.Mvc.Abstractions;
// using Microsoft.AspNetCore.Mvc.Filters;
// using System.Collections.Generic;
// using OnlineMarkerCW.Data;
//
// namespace OnlineMarkerCW.UnitTests.Controllers
// {
//     public class HomeController_UnitTests
//     {
//          private readonly OnlineMarkerCW.Controllers.TeacherController _teacherController;
//
//          public HomeController_UnitTests()
//          {
//              _teacherController = new OnlineMarkerCW.Controllers.TeacherController();
//          }
//
//         [Theory]
//         [InlineData("Student")]
//         [InlineData("Teacher")]
//         public void IndexRedirectsAccordingToRole(String role)
//         {
//             // --Arrange--
//             //-Controller
//             var controller = new HomeController(null, new LoggerFactory(), null, null);
//             //-User
//             var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//             {
//                  new Claim(ClaimTypes.NameIdentifier, "1"),
//                  new Claim("Name", "Stuart"),
//                  new Claim(ClaimTypes.Surname, "Dent"),
//                  new Claim(ClaimTypes.Email, "some@email.com"),
//                  new Claim(ClaimTypes.Role, role)
//             }));
//
//             //Create a HttpContext
//             var testHttpCtxt = new DefaultHttpContext() { User = user };
//             //Create ActionExecutingContext.
//             var actionContext = new ActionContext(testHttpCtxt, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
//             var ListFilterMetaData = new List<IFilterMetadata>() { new Mock<IFilterMetadata>().Object };
//             var actionExecutingContext = new ActionExecutingContext(actionContext, ListFilterMetaData, new Dictionary<string, object>(), null);
//
//             //Set home controller to use mock context
//             var controllerContext = new ControllerContext(new ActionContext(
//                 testHttpCtxt,
//                 new RouteData(),
//                 new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor(),
//                 new ModelStateDictionary()));
//             controller.ControllerContext = controllerContext;
//
//             //Force use of ActionExectuing Context.
//             controller.OnActionExecuting(actionExecutingContext);
//
//             // Act
//             var result = controller.Index();
//
//             // Assert
//             var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
//             Assert.Equal("Home", redirectToActionResult.ControllerName);
//             //Should redirect to MyWorks if student, MyMarkings if Teacher.
//             if (role == "Student") {
//                 Assert.Equal("MyWorks", redirectToActionResult.ActionName);
//             } else {
//                 Assert.Equal("MyMarkings", redirectToActionResult.ActionName);
//             }
//         }
//
//         [Fact]
//         public async Task POST_WorkViewForMarkerAddsFeedback()
//         {
//             //Arrange
//             String newFeedback = "This is useful example feedback";
//             int newMark = 60;
//             //ApplicationUser testOwner = new ApplicationUser();
//             Work testWork = new Work();
//             testWork.WorkID = 1;
//             testWork.Feedback = "Unconstructive feedback";
//             testWork.Mark = 10;
//             //List<Work> workList = new List<Work>(1);
//             //workList.Add(testWork);
//
//             //User
//             var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//             {
//                  new Claim(ClaimTypes.NameIdentifier, "5"),
//                  new Claim("Name", "Tea"),
//                  new Claim(ClaimTypes.Surname, "Cheery"),
//                  new Claim(ClaimTypes.Email, "some@email.com"),
//                  new Claim(ClaimTypes.Role, "Teacher")
//             }));
//             ApplicationUser user = new ApplicationUser();
//
//              //Create a HttpContext
//              var testHttpCtxt = new DefaultHttpContext() { User = userClaims };
//             //Create ActionExecutingContext.
//             var actionContext = new ActionContext(testHttpCtxt, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
//             var ListFilterMetaData = new List<IFilterMetadata>() { new Mock<IFilterMetadata>().Object };
//             var actionExecutingContext = new ActionExecutingContext(actionContext, ListFilterMetaData, new Dictionary<string, object>(), null);
//
//             //-Controller and database dependencies.
//             var m_IUserStore = new Mock<IUserStore<ApplicationUser>>();
//             var m_userManager = new Mock<UserManager<ApplicationUser>>(m_IUserStore.Object, null, null, null, null, null, null, null, null);
//             m_userManager.Setup(um => um.GetUserAsync(userClaims)).ReturnsAsync<UserManager<ApplicationUser>, ApplicationUser>(user);
//
//             var m_works = new Mock<DbSet<Work>>();
//             m_works.Object.Add(testWork);
//
//             var m_dbContext = new Mock<ApplicationDbContext>();
//             m_dbContext.Setup(ctxt => ctxt.Works).Returns(m_works.Object);
//             var controller = new HomeController(m_userManager.Object, new LoggerFactory(), null, null); //TODO
//
//             //set to use mock context
//             var controllerContext = new ControllerContext(new ActionContext(
//                 testHttpCtxt,
//                 new RouteData(),
//                 new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor(),
//                 new ModelStateDictionary()));
//             controller.ControllerContext = controllerContext;
//
//             //Force use of ActionExectuing Context.
//             controller.OnActionExecuting(actionExecutingContext);
//
//             //Act
//             ViewResult result = (ViewResult) await controller.WorkViewForMarker(testWork.WorkID, newFeedback, newMark);
//
//             //Assert
//             Work retrievedWork = (Work) result.Model;
//             Assert.Equal(newFeedback, retrievedWork.Feedback);
//             Assert.Equal(newMark, retrievedWork.Mark);
//             Assert.Equal(user, retrievedWork.Marker);
//         }
//
//     }
// }
