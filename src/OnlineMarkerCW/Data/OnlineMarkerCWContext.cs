using OnlineMarkerCW.Models;
using Microsoft.EntityFrameworkCore;

namespace OnlineMarkerCW.Data
{
    public class OnlineMarkerCWContext : DbContext
    {
        public OnlineMarkerCWContext(DbContextOptions<OnlineMarkerCWContext> options) : base(options)   { }
        //public DbSet<User> Users { get; set; }
        public DbSet<Work> Works { get; set; }

    }
}
