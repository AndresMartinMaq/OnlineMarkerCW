using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;
using OnlineMarkerCW.ViewModels;

namespace OnlineMarkerCW.Controllers
{

    public class HomeController : Controller
    {

      private readonly UserManager<ApplicationUser> _userManager; //user user manager to manage session for different users
      private readonly ILogger _logger;//logger for debuging.
      private string user_role;

      //if overriden, this method will be called for every action method in the class
      public override void OnActionExecuting(ActionExecutingContext filterContext)
      {
        base.OnActionExecuting(filterContext);
          //store claim values in view data. Claims dont access the DB, but rather the indenity model
          ViewData["user_id"]      = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
          ViewData["user_email"]   = this.User.FindFirstValue(ClaimTypes.Email);
          ViewData["user_name"]    = this.User.FindFirstValue("Name");
          ViewData["user_surname"] = this.User.FindFirstValue(ClaimTypes.Surname);
          user_role                = this.User.FindFirstValue(ClaimTypes.Role);
          ViewData["user_role"]    = user_role;
      }


      //init the controller with user manager
      public HomeController(UserManager<ApplicationUser> userManager,ILoggerFactory loggerFactory)
      {
          _userManager = userManager;
          _logger = loggerFactory.CreateLogger<AccountController>();

      }


      [Authorize]
      public IActionResult Index()
      {
        //session example
        // ViewData["string-from-session"] = HttpContext.Session.GetString("session_mail");

        //heck for the user role example
        //_userManager.IsInRoleAsync(user,"Student").Result);

          if(user_role == "Teacher")
          {
              ViewData["Welcome-Message"] = "you're an almighty teacher.";
          }

          if(user_role == "Student")
          {
              ViewData["Welcome-Message"] = "You're a bloody student, aren't ya.";
          }
          return View();
      }

      [Authorize(Roles = "Student")] //you can use authorisation based on the role - quite convient to seperate between teachers and students views
        public IActionResult MyWorks() {
          return View();
        }

        [Authorize]
        public IActionResult About()  {
          return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
