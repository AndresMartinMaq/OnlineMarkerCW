using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

//https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#authorization-filters
namespace OnlineMarkerCW.Filters
{
//Create a custom annotation which will allow accesing controller only for non loged in users.
  public class AnonymousOnly: ActionFilterAttribute
  {

      //public override Task OnAuthorizationAsync(. context)
      public override void OnActionExecuting(ActionExecutingContext filterContext)
      {
          //if user not logged in, continue
          if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
          {
                base.OnActionExecuting(filterContext);
          } else {
            //if user logged in, redirect
            filterContext.HttpContext.Response.Redirect(Uri.EscapeUriString("/Home/Index"));
          }


      }
  }
}
