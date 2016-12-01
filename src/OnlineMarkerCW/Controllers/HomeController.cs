using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineMarkerCW.Models;
using OnlineMarkerCW.ViewModels;
using Microsoft.AspNetCore.Hosting;
using OnlineMarkerCW.Data;
using Microsoft.EntityFrameworkCore;
using OnlineMarkerCW.Services;
using OnlineMarkerCW.Interfaces;

namespace OnlineMarkerCW.Controllers
{

    public class HomeController : Controller
    {

      private readonly UserManager<ApplicationUser> _userManager; //user user manager to manage session for different users
      private readonly ILogger _logger;//logger for debuging.
      private string user_role;
      private string user_ID;
      private IHostingEnvironment _hostingEnv;//required for serving uploaded files
      private readonly IDbServices _dbServices;//dbservice methods. Contains db context.

      //if overriden, this method will be called for every action method in the class
      public override void OnActionExecuting(ActionExecutingContext filterContext)
      {
          //store claim values in view data. Claims dont access the DB, but rather the indenity model
          user_ID                  = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
          ViewData["user_id"]      = user_ID;
          ViewData["user_email"]   = this.User.FindFirstValue(ClaimTypes.Email);
          ViewData["user_name"]    = this.User.FindFirstValue("Name");
          ViewData["user_surname"] = this.User.FindFirstValue(ClaimTypes.Surname);
          user_role                = this.User.FindFirstValue(ClaimTypes.Role);
          ViewData["user_role"]    = user_role;
          base.OnActionExecuting(filterContext);
      }


      //injec the controller with user manager, logger and hosting manger
      public HomeController(UserManager<ApplicationUser> userManager,ILoggerFactory loggerFactory, IHostingEnvironment hostingEnv, IDbServices dbServices)
      {
          _userManager = userManager;
          _logger = loggerFactory.CreateLogger<HomeController>();
          _hostingEnv = hostingEnv;
          _dbServices = dbServices;
      }

      [HttpGet]
      [Authorize]
      public IActionResult Index()
      {

          //check for the user role and then redirect to the relevand MyWorks page for Student and MyMarkings page for the teacher.
          if(user_role == "Teacher")
          {
              return RedirectToAction(nameof(HomeController.MyMarkings), "Home");
          }

          if(user_role == "Student")
          {
              return RedirectToAction(nameof(HomeController.MyWorks), "Home");
          }
          return View();

      }

      [HttpGet]
      [Authorize(Roles = "Student")]
      //for student only fetch and display his own submitted works
        public async Task<IActionResult> MyWorks() {
          var user = await _userManager.GetUserAsync(this.User);
          var model = await _dbServices.GetSubmitedWorks(user);
          return View("MyWorks",model);
        }

       [HttpPost]
       [ValidateAntiForgeryToken]
       [Authorize(Roles = "Student")]
       //handler for upploading an html file
       public async Task<IActionResult> MyWorks(MyWorksViewModel viewModel)
       {
         //prearage the file and path where to save it
         var file = viewModel.File;
         DateTime localDate = DateTime.Now;
         string timeNow = localDate.ToString("yyyyMMddHHmmss");
         string upload_string = "html_uploads/"  + user_ID;
         var uploads = Path.Combine(_hostingEnv
           .WebRootPath, upload_string);
         //check if directry exists, if not, create one.
         if (!Directory.Exists(uploads)) {
            Directory.CreateDirectory(uploads);
         }
         //get the current user
            var user = await _userManager.GetUserAsync(this.User);
          //check the modelstate
          if (ModelState.IsValid) {
           //check that file is not empty and in the right extension
               if (file != null && file.Length > 0 &&  file.FileName.EndsWith(".html")) {
                         using (var fileStream = new FileStream(Path.Combine(uploads,  timeNow + "_" +  file.FileName), FileMode.Create))
                         {
                           try  {
                             //save a db entry to the db
                             Work work = new Work ();
                             work.FileName = file.FileName;
                             work.FilePath = Path.Combine(uploads,  timeNow + "_" +  file.FileName);
                             work.SubmitDate = localDate;
                             work.Owner = user;
                             _dbServices.AddWork(work);
                             //save the file stream to the file
                             await file.CopyToAsync(fileStream);
                             ViewData["upload-message"] = "File upload sucessfull";
                          } catch (Exception ex) {
                            //if there are any expection, catch the Error messages
                            ModelState.AddModelError("File", "Upload failed: " +ex.Message.ToString());
                          }
                       }
                }
                //Upload failed, please try again and  check the file size and extention
                else {
                  ModelState.AddModelError("File",  "Upload failed, please try again, don't forget to check the file size and extension.");
                }
              }

              //get the list of the submitted works by the user and return a view with them
              var model = await _dbServices.GetSubmitedWorks(user);
              return View("MyWorks", model);
       }

       [HttpPost]
       [ValidateAntiForgeryToken]
       [Authorize(Roles = "Student")]
       //this handler hanles the post requeds to delete the work
       public async Task<IActionResult> WorkDelete(int ID)  {
        var work =  await _dbServices.GetWorkWithID(ID);
        var user = await _userManager.GetUserAsync(this.User);
        //check if owner is sending the request to delete, if so delete the file physically and from the db
        if (work.Owner == user) {
            _dbServices.RemoveWork(work);
            System.IO.File.Delete(work.FilePath);
        }
        //redirect ot home
        return RedirectToAction(nameof(HomeController.MyWorks), "Home");
      }

       [Authorize]
       [HttpGet]
       public async Task<IActionResult> WorkView(int ID)  {
         //fetch the work requested  and current user
        var work = await _dbServices.GetWorkWithID(ID);
        var user = await _userManager.GetUserAsync(this.User);
        //check if owner or teacher tries to access the page. If he is return the WorkView, if not rerired to access denied page.
        if (work?.Owner == user || user_role == "Teacher") {
           return View("WorkView",work);
        } else {
          return Redirect("/Error_Message/403");
        }

      }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> MyMarkings()  {
            var user = await _userManager.GetUserAsync(this.User);
            //Include Owner tells the framework to load the foreign key field Owner, otherwise it will be null.
            var model = await _dbServices.GetWorksAndOwners();
            return View("MyMarkings",model);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> WorkViewForMarker(int ID)
        {
            var work = await _dbServices.GetWorkWithID(ID);
            var user = await _userManager.GetUserAsync(this.User);
            //check if owner or teacher tries to access the page
            if (user_role == "Teacher"){
                return View(work);
            }
            else{
                return Redirect("/Error_Message/403");
            }
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Update the Work in db to include new mark and feedback.
        public async Task<IActionResult> WorkViewForMarker(int id, MarkingViewModel viewModel)
        {

            //Write to db.
            var work = await _dbServices.GetWorkWithID(id);
            var user = await _userManager.GetUserAsync(this.User);
            if (ModelState.IsValid)
            {
                _dbServices.MarkWork(work, user, viewModel.feedback, viewModel.mark);
                ViewData["update-confirmation-msg"] = "Feedback and Mark Updated Successfully";
            } else {
                ViewData["input-feedback-preservation"] = viewModel.feedback;
            }
            return View(work);
        }

        [Authorize]
        [HttpGet]
        public IActionResult About()  {
          return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

          //The universal method for displaying errors
          [Authorize]
          [HttpGet]
          [Route("/Error_Message/{code}")]
          public IActionResult Error_Message(int code) {
            //if 403 return access denied page
          if (code == 403 ) {
            ViewData["Message"] = "You are not allowed to access this page, please try another one.";
            ViewData["Tittle"] = "403 Access Denied";
        } else if ( code == 404) {
          //if page not found, return 404
            ViewData["Message"] = "Page not found.";
            ViewData["Title"] = "404 Not Found";
        } else {
          //if any otehr code, return the code
          ViewData["Title"] = code.ToString();
          ViewData["Message"] = code.ToString();
        }
            return View("Message_or_error");
          }
    }
}
