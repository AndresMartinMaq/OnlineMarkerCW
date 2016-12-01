using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace OnlineMarkerCW.ViewModels
{
  //Define a Viewmodel for the File Upload post request and the relevand Validaiton requirements
  public class MyWorksViewModel {

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }

    }

    public class MarkingViewModel
    {

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 20)]
        public String feedback { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "The mark must be between 0 and 100 (inclusive).")]
        public int mark { get; set; }

    }
}
