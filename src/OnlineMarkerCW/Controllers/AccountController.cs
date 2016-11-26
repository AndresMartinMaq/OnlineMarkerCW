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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;
using OnlineMarkerCW.ViewModels;
using OnlineMarkerCW.Services;
using OnlineMarkerCW.Interfaces;
using OnlineMarkerCW.Filters;

namespace OnlineMarkerCW.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager; //object for creating and managins users
        private readonly SignInManager<ApplicationUser> _signInManager;//object for singing in the user and creating a session
        private readonly RoleManager<ApplicationUserRole> _roleManager; //role manager for managing student/teacher roles
        private readonly ILogger _logger;//logger for debuging.
        private readonly IDbServices _dbServices;//dbservice methods.

        public AccountController( UserManager<ApplicationUser> userManager,  SignInManager<ApplicationUser> signInManager,RoleManager<ApplicationUserRole> roleManager, ILoggerFactory loggerFactory,IDbServices dbServices )
        {
          _userManager = userManager;
          _signInManager = signInManager;
          _roleManager = roleManager;
          _logger = loggerFactory.CreateLogger<AccountController>();
          _dbServices = dbServices;
        }

        //GET: /Account/Login
        [HttpGet]
        [AnonymousOnly]
        public IActionResult Login(string returnUrl = null)
        {
            //save redirect data in the view, so that it can preserved for a post request
            ViewData["ReturnUrl"] = returnUrl;
            return View("Login");
        }

        //GET: /Account/Register
        [HttpGet]
        [AnonymousOnly]
        public IActionResult Register()
        {
          //Generated a ViemModel instance so that a select list is created
          RegisterViewModel model = new RegisterViewModel();
          return View(model);

        }

        //POST: /Account/Register
        [HttpPost]
        [AnonymousOnly]
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

                var result = await _userManager.CreateAsync(user, model.Password); //await in asyn mode for # a user
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
                          IdentityResult roleResult = _roleManager.CreateAsync(role).Result;
                          if(!roleResult.Succeeded)
                          {
                            AddErrors(roleResult);
                            return View(model);
                          }
                      }
                    //log things into session
                    /*HttpContext.Session.SetString("email", model.Email);
                    HttpContext.Session.SetString("name",  model.Name);
                    HttpContext.Session.SetString("surname", model.Surname);
                    HttpContext.Session.SetString("role", userType);*/
                    //store claims
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                    await _userManager.AddClaimAsync(user, new Claim("Name",  model.Name));
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, model.Surname));
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, userType));

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
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //dont use session, use claims, claims store the information under the cookie rather than database, hence work as same way as session
                    _logger.LogInformation(1, "User logged in and stulst is {result}", result);
                    if (Url.IsLocalUrl(returnUrl))
                      {
                        return Redirect(returnUrl);
                      }
                      return RedirectToAction(nameof(HomeController.Index), "Home");


                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View("Login", model);
                }
            }

            //if modelstate is wrong, make sure it is dumped out.
            return View("Login", model);
        }

        //POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            //HttpContext.Session.Abandon();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult AccessDenied()
        {
          return Redirect("/Error_Message/403");
        }

        //helper functions
        //add errors for the vallidation error summaary
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }



    }
}
