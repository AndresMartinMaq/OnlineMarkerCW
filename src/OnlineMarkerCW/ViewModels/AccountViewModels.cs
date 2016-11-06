using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineMarkerCW.Models;

namespace OnlineMarkerCW.ViewModels
{


    public class RegisterViewModel
    {

      public RegisterViewModel()
      {
        //prepoulate the list choises whener the viemodel is used
        List<SelectListItem> list = new List<SelectListItem>();
        var roles = new[]{
                 new SelectListItem{Value = "0", Text = "Student" },
                 new SelectListItem{Value = "1", Text = "Teacher" },
             };
        list = roles.ToList();
        UserTypeList = list;
      }

      [Required]
      [DataType(DataType.EmailAddress)]
      [Display(Name = "Email")]
      public string Email { get; set; }
      [Required]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
      [Display(Name = "Name")]
      public string Name { get; set; }
      [Required]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
      [Display(Name = "Surname")]
      public string Surname { get; set; }
      [Required]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "Password")]
      public string Password { get; set; }
      [Required]
      [DataType(DataType.Password)]
      [Display(Name = "Confirm Password")]
      [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
      public string ConfirmPassword { get; set; }
      //[Required]
      [Display(Name = "User Type")]
      [Range (0,1, ErrorMessage = "The type should be either a student or a teacher")]
      public int UserTypeID { get; set; }

      //list for generating the user type for list selection
      public List<SelectListItem> UserTypeList { get; set; }

    }

    public class LoginViewModel
    {
        [DataType(DataType.EmailAddress)]
        [Required]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        public bool RememberMe { get; set; }
    }

}
