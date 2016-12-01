using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineMarkerCW.Models
{

    // Define the applicaiton User Entity and its parameters
    public class ApplicationUser : IdentityUser
    {
      [Required]
      public string Name    { get; set; }
      [Required]
      public string Surname { get; set; }
    }

    //add role class for detemining role of the user
    public class ApplicationUserRole : IdentityRole
    {
      public string Description { get; set; }
    }
}
