using System;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using OnlineMarkerCW.Models;
using OnlineMarkerCW.Controllers;
using OnlineMarkerCW.Interfaces;
using OnlineMarkerCW.Filters;
using OnlineMarkerCW.UnitTests.ObjectPrintHelper;

namespace OnlineMarkerCW.UnitTests.Filters
{
    public class Filters_UnitTests
    {
      //Initiate the ouput print for the test if it fails, if test passes, no ouput is being printed.
      private readonly ITestOutputHelper output;
      private ObjectPrinter objectPrinter;

      public Filters_UnitTests(ITestOutputHelper output)  {
        this.objectPrinter = new ObjectPrinter (output);
        this.output = output;}

         [Fact]
         public void  AnonymousOnly_ActionFilterRedirectsToHomeIndex_WhenUserIsLoggedIn() {
           //**********
           //*Arrange**
           //**********
           //Firslty Mock an HttpContext with a user with an authenticated setting
            var m_Identity = new Mock<IIdentity>();
            var m_User  = new Mock<ClaimsPrincipal>();
            m_Identity.Setup(identity => identity.IsAuthenticated).Returns(true);
            m_User.Setup(user => user.Identity).Returns(m_Identity.Object);
            var m_HtttpContext = new DefaultHttpContext(){User = m_User.Object};
            //Create everything nexeccsary for the ActionExecutingContext and create it as well.
            var actionContext           = new ActionContext( m_HtttpContext,  new RouteData(),new ActionDescriptor(), new ModelStateDictionary());
            var ListFilterMetaData      = new List<IFilterMetadata>() {new Mock<IFilterMetadata>().Object};
            var actionExecutingContext  = new ActionExecutingContext(actionContext, ListFilterMetaData, new Dictionary<string, object>(), null);
            //Initiate the filter
            var _anonymousOnlyFilter    = new AnonymousOnly();
            //**********
            //***Act****
            //**********
            //Print the object before the filter applied
            output.WriteLine("#################################");
            output.WriteLine("Printing object before the filter");
            output.WriteLine("#################################");
            objectPrinter.printObject(actionExecutingContext.HttpContext.Response.StatusCode);
            objectPrinter.printObject(actionExecutingContext.HttpContext.Response.Headers);
            //Invoke the filter on actionExecutionContent
            _anonymousOnlyFilter.OnActionExecuting(actionExecutingContext);
            //Print the object after the filter applied
            output.WriteLine("#################################");
            output.WriteLine("Printing object after the filter");
            output.WriteLine("#################################");
            objectPrinter.printObject(actionExecutingContext.HttpContext.Response.StatusCode);
            objectPrinter.printObject(actionExecutingContext.HttpContext.Response.Headers);
            //**********
            //**Assert**
            //**********
            //When user is authenicated, asert that the response should be a redirect to the HomeInde
            Assert.Equal(actionExecutingContext.HttpContext.Response.StatusCode, 302);
            Assert.Equal(actionExecutingContext.HttpContext.Response.Headers["Location"], "/Home/Index");
         }

         [Fact]
         public void  AnonymousOnly_ActionFilterContinuesExecution_WhenUserNotLoggedIn() {
           //**********
           //*Arrange**
           //**********
            //Create everything nexeccsary for the ActionExecutingContext and create it as well.
            var actionContext           = new ActionContext( new DefaultHttpContext(),  new RouteData(),new ActionDescriptor(), new ModelStateDictionary());
            var ListFilterMetaData      = new List<IFilterMetadata>() {new Mock<IFilterMetadata>().Object};
            var actionExecutingContext  = new ActionExecutingContext(actionContext, ListFilterMetaData, new Dictionary<string, object>(), null);
            //Initiate the filter
            var _anonymousOnlyFilter    = new AnonymousOnly();
            //**********
            //***Act****
            //**********
            //Print the object before the filter applied
            output.WriteLine("#################################");
            output.WriteLine("Printing object before the filter");
            output.WriteLine("#################################");
            objectPrinter.printObject(actionExecutingContext);
            //Invoke the filter on actionExecutionContent
            _anonymousOnlyFilter.OnActionExecuting(actionExecutingContext);
            //Print the object after the filter applied
            output.WriteLine("#################################");
            output.WriteLine("Printing object after the filter");
            output.WriteLine("#################################");
            objectPrinter.printObject(actionExecutingContext);
            //**********
            //**Assert**
            //**********
            //Try to compare the actionExecutingContext before and after filter, but that required copying exact same object, which cannot be implemted due to using predefined classes, hence check the header ans respone code again.
            //When user is not authenicated, asert that the response should bee 200, and that header is empty
            Assert.Equal(actionExecutingContext.HttpContext.Response.StatusCode, 200);
            Assert.Equal(actionExecutingContext.HttpContext.Response.Headers.Count, 0);
         }


    }
}
