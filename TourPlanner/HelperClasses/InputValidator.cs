using log4net;
using System;
using TourPlannerClasses.Models;

namespace TourPlanner.UI.HelperClasses
{
    public class InputValidator
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(InputValidator));

        public InputValidator() { }

        public string ValidateTourInput(Tours tourToValidate)
        {
            if (tourToValidate == null)
            {
                _log.Warn("Validation failed: Tour is null.");
                return "Tour cannot be null!";
            }

            if (string.IsNullOrWhiteSpace(tourToValidate.Name))
            {
                _log.Warn("Validation failed: Tour name is empty.");
                return "Tour Name cannot be empty!";
            }

            if (string.IsNullOrWhiteSpace(tourToValidate.From))
            {
                _log.Warn("Validation failed: Tour origin is empty.");
                return "Tour origin cannot be empty!";
            }

            if (string.IsNullOrWhiteSpace(tourToValidate.To))
            {
                _log.Warn("Validation failed: Tour destination is empty.");
                return "Tour Destination cannot be empty!";
            }

            if (string.IsNullOrWhiteSpace(tourToValidate.Description))
            {
                _log.Warn("Validation failed: Tour description is empty.");
                return "Tour Description cannot be empty!";
            }

            _log.Debug($"Tour input validated successfully: {tourToValidate.Name}");
            return "";
        }

        public string ValidateTourlogInput(Tourlog tourlogToValidate)
        {
            if (tourlogToValidate == null)
            {
                _log.Warn("Validation failed: Tourlog is null.");
                return "Tourlog cannot be null!";
            }

            if (string.IsNullOrWhiteSpace(tourlogToValidate.Author))
            {
                _log.Warn("Validation failed: Author is empty.");
                return "Author cannot be empty!";
            }

            if (tourlogToValidate.Date == default)
            {
                _log.Warn("Validation failed: Date is not set.");
                return "Date must be set!";
            }

            if (string.IsNullOrWhiteSpace(tourlogToValidate.Comment))
            {
                _log.Warn("Validation failed: Comment is empty.");
                return "Comment cannot be empty!";
            }

            if (!Enum.IsDefined(typeof(Difficulty), tourlogToValidate.Difficulty))
            {
                _log.Warn("Validation failed: Invalid difficulty value.");
                return "Difficulty has not been properly set!";
            }

            if (tourlogToValidate.TotalDistance <= 0 || !double.IsRealNumber(tourlogToValidate.TotalDistance))
            {
                _log.Warn($"Validation failed: Invalid distance ({tourlogToValidate.TotalDistance}).");
                return "Distance is either not a number or 0!";
            }

            if (tourlogToValidate.TotalTime <= TimeSpan.Zero)
            {
                _log.Warn("Validation failed: Total time must be positive.");
                return "Total time spent must be positive!";
            }

            if (tourlogToValidate.Rating < 1 || tourlogToValidate.Rating > 5)
            {
                _log.Warn($"Validation failed: Rating out of range ({tourlogToValidate.Rating}).");
                return "Rating must be between 1 and 5!";
            }

            _log.Debug($"Tourlog input validated successfully for author: {tourlogToValidate.Author}");
            return "";
        }
    }
}
