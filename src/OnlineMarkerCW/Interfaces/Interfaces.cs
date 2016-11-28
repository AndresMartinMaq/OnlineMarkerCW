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

namespace OnlineMarkerCW.Interfaces
{
    public interface IDbServices
    {
        Task<List<Work>> GetSubmitedWorks(ApplicationUser Owner);
        Task<Work> GetWorkWithID(int id);
        Task<List<Work>> GetWorksAndOwners();
        void AddWork(Work work);
        void RemoveWork(Work work);
        void MarkWork(Work work, ApplicationUser marker, String feedback, int mark);
    }
}
