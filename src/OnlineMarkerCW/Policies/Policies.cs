using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using OnlineMarkerCW.Controllers;

//https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#authorization-filters
//https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies
namespace OnlineMarkerCW.Policies
{

//define an interface for dependecy injection
  public class AnonymousOnlyRequirement : IAuthorizationRequirement
  {
  }


//Create a custom annotation which will allow accesing controller only for non loged in users.
  public class AnonymousOnlyHandler: AuthorizationHandler<AnonymousOnlyRequirement>
  {

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AnonymousOnlyRequirement requirement)
    {
          //if user is not logged in, succeed
          if (!context.User.Identity.IsAuthenticated)
          {
              context.Succeed(requirement);
          }

          //esle finish the task redirect and finish the task
          // RedirectToActionResult redirect_to_action = new RedirectToActionResult(nameof(HomeController.Index), "Home", null);
          // redirect_to_action.ExecuteResult(context.User);
          //HttpContext.Response.Redirect("/");
          return Task.CompletedTask;
      }
  }
}
