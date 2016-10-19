using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
      //init the controller with user manager
      public HomeController(UserManager<ApplicationUser> userManager,ILoggerFactory loggerFactory)
      {
          _userManager = userManager;
          _logger = loggerFactory.CreateLogger<AccountController>();
      }


      [Authorize]
      public IActionResult Index()
      {
          //get the user
          ApplicationUser user = _userManager.GetUserAsync
                               (HttpContext.User).Result;
          //get user's role
          ViewData["Name"] = $"{user.Name}!";
            _logger.LogWarning(3, "result of _userManager.IsInRoleAsync(user,'Teacher').Result is  {result}", _userManager.IsInRoleAsync(user,"Teacher").Result);
          if(_userManager.IsInRoleAsync(user,"Teacher").Result)
          {
              ViewData["Welcome-Message"] = "you're an almighty teacher.";
          }
            _logger.LogWarning(3, "result of _userManager.IsInRoleAsync(user,'Student').Result is  {result}", _userManager.IsInRoleAsync(user,"Student").Result);
          if(_userManager.IsInRoleAsync(user,"Student").Result)
          {
              ViewData["Welcome-Message"] = "You're a bloody student, aren't ya.";
          }
          return View();
      }

        public IActionResult Error()
        {
            return View();
        }
    }
}
