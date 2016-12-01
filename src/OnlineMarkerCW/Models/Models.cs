using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineMarkerCW.Models{


// Enity model for a piece of work submission
    public class Work{

        //[key]
        public int WorkID { get; set; }

        //File path and FileName for the HTML file representing the Work upload
        public string FilePath { get; set; }
        public string FileName {get; set;}

        //Mark and Feedback of the Work submitted by the teacher
        public int Mark{ get; set; }
        public bool Marked{ get; set; } = false;
        public string Feedback {get; set;}

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: HH:mm dd/MM/yyyy}")]
        public DateTime SubmitDate{ get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: HH:mm dd/MM/yyyy}")]
        public DateTime MarkDate{ get; set; }

        //Define OWnder and the MArker of the file
        public ApplicationUser Owner{ get; set; }
        public ApplicationUser Marker{ get; set; }

    }
}
