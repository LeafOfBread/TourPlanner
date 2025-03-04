
using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Markup;

namespace TourPlannerClasses.Models
{
    public class TourList
    {
        public List<Tours> AllTours { get; set; }
        public TourList() { }
    }

    public class Tours
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string imagePath { get; set; }
        public TimeSpan duration { get; set; }
        public double distance { get; set; }

        public Tours() { }

        public Tours(string Name, string Description, string StartLocation, string EndLocation, string imagePath, TimeSpan Duration, double Distance)
        {
            name = Name;
            description = Description;
            from = StartLocation;
            to = EndLocation;
            duration = Duration;
            distance = Distance;
            this.imagePath = imagePath;
        }
    }
}
