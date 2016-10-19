using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace OnlineMarkerCW.Models
{

    public enum UserTypeEnum
    {
        Student,Teacher
    }


    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
      public string Name    { get; set; }
      public string Surname { get; set; }
      //public UserType? UserType { get; set; }
    }

    //add role class for detemining role of the user
    public class ApplicationUserRole : IdentityRole
    {
      public string Description { get; set; }
    }
}
