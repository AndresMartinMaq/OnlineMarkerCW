using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineMarkerCW.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;

namespace OnlineMarkerCW.ViewModels
{


    public class WorkUploadViewModel
    {

      public WorkUploadViewModel() {}

      [Required(ErrorMessage ="Please Upload a HTML File")]
      [DataType(DataType.Upload)]
      [Display(Name ="Upload the html file for marking")]
      //looks like there is bug which does not corretly check the file type
      //[FileExtensions(Extensions ="html,htm", ErrorMessage = "Upload failed. Please make sure that file extension is .html")]
      public IFormFile File { get; set; }

    }
/*
  public class WorkMarkingViewModel
  {

    public WorkUploadViewModel()  {}

  }
*/

}
