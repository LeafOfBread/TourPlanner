using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.Models;

namespace TourPlanner.UI.HelperClasses
{
    public class InputValidator
    {
        public InputValidator() { }

        public string ValidateTourInput(Tours tourToValidate)
        {
            if (tourToValidate == null)
                return "Tour cannot be null!";
            if (string.IsNullOrWhiteSpace(tourToValidate.Name))
                return "Tour Name cannot be empty!";
            if (string.IsNullOrWhiteSpace(tourToValidate.From))
                return "Tour origin cannot be empty!";
            if (string.IsNullOrWhiteSpace(tourToValidate.To))
                return "Tour Destination cannot be empty!";
            if (string.IsNullOrWhiteSpace(tourToValidate.Description))
                return "Tour Description cannot be empty!";

            else return "";
        }

        public string ValidateTourlogInput(Tourlog tourlogToValidate)
        {
            if (tourlogToValidate == null)
                return "Tourlog cannot be null!";
            if (string.IsNullOrWhiteSpace(tourlogToValidate.Author))
                return "Author cannot be empty!";
            if (tourlogToValidate.Date == default)
                return "Date must be set!";
            if (string.IsNullOrWhiteSpace(tourlogToValidate.Comment))
                return "Comment cannot be empty!";
            if (!Enum.IsDefined(typeof(Difficulty), tourlogToValidate.Difficulty))
                return "Difficulty has not been properly set!";
            if (tourlogToValidate.TotalDistance <= 0 || !double.IsRealNumber(tourlogToValidate.TotalDistance))
                return "Distance is either not a number or 0!";
            if (tourlogToValidate.TotalTime <= TimeSpan.Zero)
                return "Total time spent must be positive!";
            if (tourlogToValidate.Rating < 1 || tourlogToValidate.Rating > 5)
                return "Rating must be between 1 and 5!";

            else return "";
        }
    }
}
