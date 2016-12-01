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

        //Inject the dependencies into the controller via the constructor
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
          return View("Register",model);

        }

        //POST: /Account/Register
        [HttpPost]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
          if (ModelState.IsValid)//checks if the incomming values from the reuqest can be mapped on the model.
            {
                ApplicationUser user = new ApplicationUser();
                user.UserName = model.Email;
                user.Email = model.Email; //use username as email
                user.Name = model.Name;
                user.Surname = model.Surname;

                var result = await _userManager.CreateAsync(user, model.Password); //await in async for user to be created
                if (result.Succeeded) //if registration succeeded, singin the user and redirect home
                {
                  string userType;
                  userType = model.UserTypeID == 0 ? "Student" : "Teacher"; //check what value is passed from the request, if UserTypeID is 0 it is student, esle it is Teacher
                  if(!_roleManager.RoleExistsAsync(userType).Result) //if role does not exists, create a new one.
                      {
                          ApplicationUserRole role = new ApplicationUserRole();
                          role.Name = userType;
                          role.Description = userType;
                          var roleResult = _roleManager.CreateAsync(role).Result;
                          if(!roleResult.Succeeded)
                          {
                            AddErrors(roleResult);
                            return View("Register",model);
                          }
                      }
                    //store claims about the suer, so that they can be accessed from the controllers ContextData without having to read the db (like a session)
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                    await _userManager.AddClaimAsync(user, new Claim("Name",  model.Name));
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, model.Surname));
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, userType));

                    await _userManager.AddToRoleAsync(user,userType); //add the user to the role

                    await _signInManager.SignInAsync(user, isPersistent: false); //sing in the user

                    return RedirectToAction(nameof(HomeController.Index), "Home"); //redirect to home page
                }
                //If creating user is unsuscefull, add errors to the ModelState
                AddErrors(result);
            }

            // If there is a fail, redisplay the form with the relevand model errors
            return View("Register",model);
        }

        //POST: /Account/Login
        [HttpPost]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            //save redirect data in the view, so that it can preserved for the next post request if the first one fails
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //sing in the user using the ViewModel data, if is is uscesfull move Home or to the redirect URL
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //Check if there is a redirect data passed, if so, Redirect
                    if (Url.IsLocalUrl(returnUrl))
                      {
                        _logger.LogInformation(1, "redirec to  {returnUrl}", returnUrl);
                        return Redirect(returnUrl);
                      }
                      //Redirect home
                      return RedirectToAction(nameof(HomeController.Index), "Home");


                }
                else
                {
                    //Add error to the modelstaet if attempt not  susscesfull
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            //if something wrong, redisplay the page and dump the modelstate errors
            return View("Login", model);
        }

        //POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            //sign out the user out of the system
            await _signInManager.SignOutAsync();
            //Redirect user to the login page
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        //Redirerect to the 403 page if access Denied
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
