using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OnlineMarkerCW.Models;

namespace OnlineMarkerCW.Controllers
{
    public class TeacherController : Controller
    {
        // GET: /Teacher/
        //[Authorize]
        public IActionResult Index(){
            ApplicationUser u1 = new ApplicationUser { Name = "Alice", Surname = "Smith" };
            ApplicationUser u2 = new ApplicationUser { Name = "Bob", Surname = "Baker" };
            Work[] testmodel = {
                new Work {WorkID = 0, Mark = 68, Owner = u1},
                new Work {WorkID = 1, Mark = -1, Owner = u1},
                new Work {WorkID = 2, Mark = 80, Owner = u1},
                new Work {WorkID = 3, Mark = 68, Owner = u2},
                new Work {WorkID = 4, Mark = 35, Owner = u2}
            };
            return View(testmodel);
        }

        // GET: /Teacher/Work/{id:int}
        //[Authorize]
        public IActionResult Work(int id)
        {
            ApplicationUser u1 = new ApplicationUser { Name = "Alice", Surname = "Smith" };
            ApplicationUser u2 = new ApplicationUser { Name = "Bob", Surname = "Baker" };
            Work[] testWorks = {
                new Work {WorkID = 0, Mark = 68, Owner = u1},
                new Work {WorkID = 1, Mark = -1, Owner = u1},
                new Work {WorkID = 2, Mark = 80, Owner = u1},
                new Work {WorkID = 3, Mark = 68, Owner = u2},
                new Work {WorkID = 4, Mark = 35, Owner = u2}
            };
            return View(testWorks[id]);
        }
    }
}
