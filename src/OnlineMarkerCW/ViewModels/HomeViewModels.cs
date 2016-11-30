using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineMarkerCW.Models;

namespace OnlineMarkerCW.ViewModels
{
  public class MyWorksViewModel {

    [Required]
    [DataType(DataType.Upload)]
    public IFormFile File { get; set; }

  }
}
