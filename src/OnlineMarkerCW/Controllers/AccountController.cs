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

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager; //object for creating and managins users
        private readonly SignInManager<ApplicationUser> _signInManager;//object for singing in the user and creating a session
        private readonly RoleManager<ApplicationUserRole> _roleManager; //role manager for managing student/teacher roles
        private readonly ILogger _logger;//logger for debuging.

        public AccountController( UserManager<ApplicationUser> userManager,  SignInManager<ApplicationUser> signInManager,RoleManager<ApplicationUserRole> roleManager, ILoggerFactory loggerFactory)
        {
          _userManager = userManager;
          _signInManager = signInManager;
          _roleManager = roleManager;
          _logger = loggerFactory.CreateLogger<AccountController>();
        }

        //GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {

          RegisterViewModel model = new RegisterViewModel();
          //model.UserTypeList = createUserTypeList();
          return View(model);

        }

        //POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
        //  model.UserTypeList = createUserTypeList(); //populate the list with values, otherwise ASP.NET MVC core thinks you are sill and stry to access an null object for some reason.
          if (ModelState.IsValid)//checks if the incomming values from the reuqest can be mapped on the model.
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = model.Email;
                user.Email = model.Email; //use username as email
                user.Name = model.Name;
                user.Surname = model.Surname;

                var result = await _userManager.CreateAsync(user, model.Password); //await in asyn mode for create a user
                if (result.Succeeded) //if registration succeeded, singin the user and redirect home
                {
                  string userType;
                  userType = model.UserTypeID == 0 ? "Student" : "Teacher"; //check what value is passed from the request, if UserTypeID is 0 it is student, esle it is Teacher
                  _logger.LogWarning(3, "Current RegisterViewModel.UserTypeID is {UserTypeList} and userType string is {userType}", model.UserTypeID,userType);
                  if(!_roleManager.RoleExistsAsync(userType).Result)
                      {
                          ApplicationUserRole role = new ApplicationUserRole();
                          role.Name = userType;
                          role.Description = userType;
                          IdentityResult roleResult = _roleManager.
                          CreateAsync(role).Result;
                          if(!roleResult.Succeeded)
                          {
                            AddErrors(roleResult);
                            return View(model);
                          }
                      }
                    await _userManager.AddToRoleAsync(user,userType); //add the user to the role
                    await _signInManager.SignInAsync(user, isPersistent: false); //sing in the user
                    _logger.LogInformation(3, "User created a new account with password.");
                    return RedirectToAction(nameof(HomeController.Index), "Home"); //redirect to home page
                }
                AddErrors(result);
            }

            // If there is a fail, redisplay the form.
            return View(model);
        }

        //POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            //if there is something wrong, redisplay the view
            return View(model);
        }

        //POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //helper functions
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.FindByIdAsync(_userManager.GetUserId(User));
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
        /*
        private List<SelectListItem> createUserTypeList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            //populate the model wtih list of  available role enums
            list.Add(new SelectListItem() { Value = "Student", Text = "Student" });
            list.Add(new SelectListItem() { Value = "Teacher", Text = "Teacher" });
            return list;

        }
        */

    }
}
