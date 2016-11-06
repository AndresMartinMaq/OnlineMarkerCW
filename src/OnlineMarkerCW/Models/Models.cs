using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

        //[key]
        public int WorkID { get; set; }

        public string FilePath { get; set; }
        public string FileName {get; set;}

        public int Mark{ get; set; }
        public bool Marked{ get; set; } = false;
        public string Feedback {get; set;}

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: HH:mm dd/MM/yyyy}")]
        public DateTime SubmitDate{ get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: HH:mm dd/MM/yyyy}")]
        public DateTime MarkDate{ get; set; }

        public ApplicationUser Owner{ get; set; }
        public ApplicationUser Marker{ get; set; }

    }
}
