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

	public class DbServices : IDbServices {

        private ApplicationDbContext _context;// db context for writing to the Works DB

	    //inject the controller with user manager, logger and hosting manger
	    public DbServices(ApplicationDbContext context){
		    _context = context;
	    }

	   //get the list of works by a current user.
	    public async Task<List<Work>> GetSubmitedWorks(ApplicationUser Owner){
		    return await _context.Works.Where(w => w.Owner == Owner).OrderBy(w => w.SubmitDate).ToListAsync();
	    }
        
        public async Task<Work> GetWorkWithID(int id){
            return await _context.Works.FirstOrDefaultAsync(w => w.WorkID == id);
        }

        //Include Owner tells the framework to load the foreign key field Owner, otherwise it will be null.
        public async Task<List<Work>> GetWorksAndOwners()
        {
            return await _context.Works.OrderBy(w => w.SubmitDate).Include(w => w.Owner).ToListAsync();
        }

        public void AddWork(Work work){
            _context.Works.Add(work);
            _context.SaveChanges();
        }

        public void RemoveWork(Work work){
            _context.Works.Remove(work);
            _context.SaveChanges();
        }

        public void MarkWork(Work work, ApplicationUser marker, String feedback, int mark) {
            _context.Update(work);
            work.Marked = true;
            work.MarkDate = DateTime.Now;
            work.Feedback = feedback;
            work.Mark = mark;
            work.Marker = marker;
            _context.SaveChanges();
        }

    }
}
