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
using Microsoft.AspNetCore.Hosting;
using OnlineMarkerCW.Data;
using Microsoft.EntityFrameworkCore;
using OnlineMarkerCW.Interfaces;

namespace OnlineMarkerCW.Services
{

    public class DbServices : IDbServices
    {


      private ApplicationDbContext _context;// db context for writing to the Works DB



      //injest the controller with user manager, logger and hosting manger
      public DbServices(ApplicationDbContext context)
      {

          _context = context;

      }

      //get the list of works by a current user.
      public async Task<List<Work>> GetSubmitedWorks(ApplicationUser Owner)
        {
             return await _context.Works.Where(w => w.Owner == Owner).OrderBy(w => w.SubmitDate).ToListAsync();

        }

    }
}
