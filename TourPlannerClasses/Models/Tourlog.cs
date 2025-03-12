using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.Models;

namespace TourPlannerClasses.Models
{
    public class Tourlog
    {
        [Key]   //primary key
        public int TourLogId { get; set; }

        [ForeignKey("Tour")] //foreign key referencing the Tour model
        public int TourId { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }

        public virtual Tours Tour { get; set; }

        public Tourlog() { }

        public Tourlog(int tourid, int tourlogid, string content, string author)
        {
            this.TourId = tourid;
            this.TourLogId = tourlogid;
            this.Content = content;
            this.Author = author;
        }
    }
}
