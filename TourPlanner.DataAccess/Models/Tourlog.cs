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
    public enum Difficulty
    {
        TooEasy,
        Easy,
        Medium,
        Hard,
        TooHard
    }

    public class Tourlog
    {
        [Key]   //primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TourLogId { get; set; }

        [ForeignKey("Tour")] //foreign key referencing the Tour model
        public int TourId { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public Difficulty Difficulty { get; set; }
        
        public double TotalDistance { get; set; }
        public TimeSpan TotalTime { get; set; }
        public int Rating { get; set; }
        public string Author { get; set; }

        public virtual Tours Tour { get; set; }

        public Tourlog() 
        {
            Date = DateTime.UtcNow;
        }

        public Tourlog(int tourlogId, int tourId, DateTime date, string comment, Difficulty difficulty, double distance, TimeSpan time, int rating, string author)
        {
            this.TourLogId = tourlogId;
            this.TourId = tourId;
            this.Date = DateTime.SpecifyKind(date, DateTimeKind.Utc); // Ensure Date is UTC
            this.Comment = comment;
            this.Difficulty = difficulty;
            this.TotalDistance = distance;
            this.TotalTime = time;
            this.Rating = rating;
            this.Author = author;
        } 
    }
}
