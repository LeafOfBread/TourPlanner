using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlannerClasses.Models;
using TourPlanner.UI.HelperClasses;

namespace UnitTests
{
    public class InputValidatorTests
    {
        InputValidator _validator = new InputValidator();

        [Fact]
        public void ValidatorShouldReturnOkOnTourValidation()
        {
            Tours newTour = new Tours()
            {
                Name = "TestTour",
                From = "Salzburg",
                To = "Eisenstadt",
                Description = "Tour by Car",
                Transport = TransportType.Car
            };

            string result = _validator.ValidateTourInput(newTour);
            string expected = "";
            Assert.Equal(result, expected);
        }

        [Fact]
        public void ValidatorShouldReturnOkOnTourlogValidation()
        {
            Tourlog newTourlog = new Tourlog()
            {
                Author = "Test Author",
                Date = DateTime.Now,
                TotalDistance = 10,
                TotalTime = TimeSpan.FromHours(1),
                Rating = 5,
                Comment = "Great Tour!",
                Difficulty = Difficulty.Medium
            };

            string result = _validator.ValidateTourlogInput(newTourlog);
            string expected = "";

            Assert.Equal(result, expected);
        }

        [Fact]
        public void ValidatorShouldReturnErrOnTourValidation()
        {
            Tours newTour = new Tours()
            {
                Name = "TestTour",
                From = "",                      //whitespace should return err message
                To = "Eisenstadt",
                Description = "Tour by Car",
                Transport = TransportType.Car
            };

            string result = _validator.ValidateTourInput(newTour);
            string expected = "Tour origin cannot be empty!";
            Assert.Equal(result, expected);
        }

        [Fact]
        public void ValidatorShouldReturnErrOnTourlogValidation()
        {
            Tourlog newTourlog = new Tourlog()
            {
                Author = " ",                           //whitespace should return err message
                Date = DateTime.Now,
                TotalDistance = 10,
                TotalTime = TimeSpan.FromHours(1),
                Rating = 3,
                Comment = "This should fail!",
                Difficulty = Difficulty.TooHard
            };

            string result = _validator.ValidateTourlogInput(newTourlog);
            string expected = "Author cannot be empty!";

            Assert.Equal(result, expected);
        }
    }
}
