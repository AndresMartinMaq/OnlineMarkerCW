using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineMarkerCW.Models{

/*
//model for a user
    public class User{

        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public UserType? UserType { get; set; }
        //public ICollection<Work> Works{ get; set; }
    }
*/
// model for a pice of work submission
    public class Work{

        public int WorkID { get; set; }
        //public blob content;
        public int Mark{ get; set; }
        public DateTime SubmitDate{ get; set; }
        public DateTime MarkDate{ get; set; }

        public ApplicationUser Owner{ get; set; }
        public ApplicationUser Marker{ get; set; }

    }
}
