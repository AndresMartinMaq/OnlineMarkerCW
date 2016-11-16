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

namespace OnlineMarkerCW.Controllers
{

    public class HomeController : Controller
    {

      private readonly UserManager<ApplicationUser> _userManager; //user user manager to manage session for different users
      private readonly ILogger _logger;//logger for debuging.
      private string user_role;
      private string user_ID;
      private IHostingEnvironment _hostingEnv;//required for serving uploaded files
      private ApplicationDbContext _context;// db context for writing to the Works DB

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


      //injest the controller with user manager, logger and hosting manger
      public HomeController(UserManager<ApplicationUser> userManager,ILoggerFactory loggerFactory, IHostingEnvironment hostingEnv, ApplicationDbContext context)
      {
          _userManager = userManager;
          _logger = loggerFactory.CreateLogger<HomeController>();
          _hostingEnv = hostingEnv;
          _context = context;

      }

      [HttpGet]
      [Authorize]
      public IActionResult Index()
      {
        //session example
        // ViewData["string-from-session"] = HttpContext.Session.GetString("session_mail");

        //Check for the user role example
        //_userManager.IsInRoleAsync(user,"Student").Result);

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
      [Authorize(Roles = "Student")] //you can use authorisation based on the role - quite convient to seperate between teachers and students views
        public async Task<IActionResult> MyWorks() {
          var user = await _userManager.GetUserAsync(this.User);
          var model = await SubmitedWorks(user);
          return View(model);
        }

       [HttpPost]
       [ValidateAntiForgeryToken]
       [Authorize(Roles = "Student")]
       //Changed the task from async to a synchrounous one, as without using ajax the sucesfull upload message cannot be sent through before the view is generated.
       //http://dotnetthoughts.net/file-upload-in-asp-net-5-and-mvc-6/
       public async Task<IActionResult> MyWorks(IFormFile file)
       {
         DateTime localDate = DateTime.Now;
         string timeNow = localDate.ToString("yyyyMMddHHmmss");
         string upload_string = "html_uploads/"  + user_ID;
         var uploads = Path.Combine(_hostingEnv
           .WebRootPath, upload_string);
         _logger.LogWarning(0, "uploads is {string}", uploads) ;
         //check if directry exists, if not, create one.
         if (!Directory.Exists(uploads)) {
            Directory.CreateDirectory(uploads);
         }
            var user = await _userManager.GetUserAsync(this.User);
            _logger.LogWarning(0, "I am getting the user") ;
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
                           _context.Works.Add(work);
                           _context.SaveChanges();
                           //save the file stream to the file
                           await file.CopyToAsync(fileStream);
                           ViewData["upload-message"] = "File upload sucessfull";
                        } catch (Exception ex) {
                          ViewData["error"] = "Upload failed: " +ex.Message.ToString(); // FIXME change to the modelstate mechanism. Perhaps change back to the async
                        }
                     }
              }
              //Upload failed, please try again and  check the file size and extention
              else {
                ViewData["error"] = "Upload failed, please try again, don't forget to check the file size and extension.";
              }
              var model = await SubmitedWorks(user);
              return View(model);
       }

       [HttpPost]
       [ValidateAntiForgeryToken]
       [Authorize(Roles = "Student")]
       public async Task<IActionResult> WorkDelete(int ID)  {
        var work =  await _context.Works.FirstOrDefaultAsync(w => w.WorkID == ID);
        var user = await _userManager.GetUserAsync(this.User);
        //check if owner is sending the request to delete
        if (work.Owner == user) {
          _context.Works.Remove(work);
          _context.SaveChanges();
          System.IO.File.Delete(work.FilePath);
        }
        return RedirectToAction(nameof(HomeController.MyWorks), "Home");
      }

       [Authorize]
       [HttpGet]
       public async Task<IActionResult> WorkView(int ID)  {
        var work = await _context.Works.FirstOrDefaultAsync(w => w.WorkID == ID);
        var user = await _userManager.GetUserAsync(this.User);
        //check if owner or teacher tries to access the page
        if (work?.Owner == user || user_role == "Teacher") {
           return View(work);
        } else {
          return Redirect("/Error_Message/403");
        }

      }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> MyMarkings()  {
            var user = await _userManager.GetUserAsync(this.User);
            //Include Owner tells the framework to load the foreign key field Owner, otherwise it will be null.
            var model = await _context.Works.OrderBy(w => w.SubmitDate).Include(w => w.Owner).ToListAsync();
            return View(model);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet]
        public async Task<IActionResult> WorkViewForMarker(int ID)
        {
            var work = await _context.Works.FirstOrDefaultAsync(w => w.WorkID == ID);
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
        public async Task<IActionResult> WorkViewForMarker(int id, String feedback, int mark)//TODO validation
        {
            //Validate input.
            //string feedbackEncoded = HtmlEncoder.Default.Encode(feedback); isn't necessary, the framework seems to take care of injections here.
            if (mark > 100 || mark < 0)
            {
                ModelState.AddModelError("Mark Error", "Mark should be a number from 0 to 100.");
            }

            //Write to db.
            var work = await _context.Works.FirstOrDefaultAsync(w => w.WorkID == id);
            var user = await _userManager.GetUserAsync(this.User);
            if (ModelState.IsValid)
            {
                _context.Update(work);
                work.Marked = true;
                work.MarkDate = DateTime.Now;
                work.Feedback = feedback;
                work.Mark = mark;
                work.Marker = user;
                _context.SaveChanges();
                ViewData["update-confirmation-msg"] = "Feedback and Mark Updated Successfully";
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

        //get the list of works by a current user.
        public async Task<List<Work>> SubmitedWorks(ApplicationUser Owner)
          {
               return await _context.Works.Where(w => w.Owner == Owner).OrderBy(w => w.SubmitDate).ToListAsync();

          }

          //The universal method for displaying errors
          [Authorize]
          [HttpGet]
          [Route("/Error_Message/{code}")]
          public IActionResult Error_Message(int code) {
          if (code == 403 ) {
            ViewData["Message"] = "You are not allowed to access this page, please try another one.";
            ViewData["Tittle"] = "403 Access Denied";
        } else if ( code == 404) {
            ViewData["Message"] = "Page not found.";
            ViewData["Title"] = "404 Not Found";
        } else {
          ViewData["Title"] = code.ToString();
          ViewData["Message"] = code.ToString();
        }
            return View("Message_or_error");
          }
    }
}
