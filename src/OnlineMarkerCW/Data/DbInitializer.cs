using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineMarkerCW.Data;

namespace OnlineMarkerCW.Models
{
    public static class DbInitializer
    {
        public static void Initialize(OnlineMarkerCWContext context)
        {
            //context.Database.EnsureCreated();
/*
            // Look for any students.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new User[]
            {
                new User { Name = "Carson",   Surname = "Alexander",Username= "CAl", UserType = UserType.teacher },
                new User { Name = "Meredith", Surname = "Alonso",Username= "MAl", UserType = UserType.student },
                new User { Name = "Arturo",   Surname = "Anand",Username= "AAn", UserType = UserType.student },
                new User { Name = "Gytis",    Surname = "Barzdukas",Username= "GBa", UserType = UserType.teacher },
                new User { Name = "Yan",      Surname = "Li",  Username= "YLi", UserType = UserType.student },
                new User { Name = "Peggy",    Surname = "Justice",Username= "PJu", UserType = UserType.teacher },
                new User { Name = "Laura",    Surname = "Norman",Username= "LNo", UserType = UserType.student },
                new User { Name = "Nino",     Surname = "Olivetto",Username= "No", UserType = UserType.teacher },
            };

            foreach (User s in users)
            {
                context.Users.Add(s);
            }
            context.SaveChanges();



            var works = new Work[]
            {
                new Work { Mark = 32,
                    SubmitDate = DateTime.Parse("2007-09-01"),
                    MarkDate = DateTime.Parse("2007-09-02"),
                    Owner  = users.Single( i => i.Surname == "Norman"),
                    Marker  = users.Single( i => i.Surname == "Olivetto") },
                new Work { Mark = 100,
                    SubmitDate = DateTime.Parse("2007-09-01"),
                    MarkDate = DateTime.Parse("2007-09-02"),
                    Owner  = users.Single( i => i.Surname == "Li"),
                    Marker  = users.Single( i => i.Surname == "Olivetto") },
                new Work { Mark = 35,
                    SubmitDate = DateTime.Parse("2007-09-01"),
                    MarkDate = DateTime.Parse("2007-09-02"),
                    Owner  = users.Single( i => i.Surname == "Anand"),
                    Marker  = users.Single( i => i.Surname == "Justice") },
                new Work { Mark = 10,
                    SubmitDate = DateTime.Parse("2007-09-01"),
                    MarkDate = DateTime.Parse("2007-09-02"),
                    Owner  = users.Single( i => i.Surname == "Alonso"),
                    Marker  = users.Single( i => i.Surname == "Alexander") }
            };

            foreach (Work w in works)
            {
                context.Works.Add(w);
            }
            context.SaveChanges();
*/

        }
    }
}
