using TourPlanner;
using TourPlannerClasses.Models;


namespace UnitTests
{
    public class ModelsTest
    {
        [Fact]
        public void Tour_Constructor_ShouldInitializeProperties()
        {
            // Arrange
            int expectedId = 100;
            string expectedName = "Mountain Trip";
            string expectedDescription = "A scenic tour in the Alps";
            string expectedFrom = "Innsbruck";
            string expectedTo = "Salzburg";
            TimeSpan expectedDuration = TimeSpan.FromHours(5);
            double expectedDistance = 120.5;
            TransportType expectedTransport = TransportType.Car;

            // Act
            var tour = new Tours(expectedId, expectedName, expectedDescription, expectedFrom, expectedTo, expectedDuration, expectedDistance, expectedTransport);

            // Assert
            Assert.Equal(expectedId, tour.Id);
            Assert.Equal(expectedName, tour.Name);
            Assert.Equal(expectedDescription, tour.Description);
            Assert.Equal(expectedFrom, tour.From);
            Assert.Equal(expectedTo, tour.To);
            Assert.Equal(expectedDuration, tour.Duration);
            Assert.Equal(expectedDistance, tour.Distance);
            Assert.Equal(expectedTransport, tour.Transport);
        }

        [Fact]
        public void Tourlog_Constructor_ShouldInitializeProperties()
        {
            // Arrange
            int expectedTourId = 9;
            int expectedTourLogId = 100;
            string expectedComment = "Amazing trip!";
            string expectedAuthor = "John Doe";
            DateTime expectedDate = DateTime.Now;
            Difficulty expectedDifficulty = Difficulty.Medium;
            int expectedRating = 9;
            double expectedDistance = 10.1;
            TimeSpan expectedTotalTime = TimeSpan.FromMinutes(100);

            // Act
            Tourlog tourlog = new Tourlog(expectedTourLogId, expectedTourId, expectedDate, expectedComment, expectedDifficulty, expectedDistance, expectedTotalTime, expectedRating, expectedAuthor);

            // Assert
            Assert.Equal(expectedTourId, tourlog.TourId);
            Assert.Equal(expectedTourLogId, tourlog.TourLogId);
            Assert.Equal(expectedComment, tourlog.Comment);
            Assert.Equal(expectedAuthor, tourlog.Author);
            Assert.Equal(expectedDate, tourlog.Date);
            Assert.Equal(expectedDifficulty, tourlog.Difficulty);
            Assert.Equal(expectedRating, tourlog.Rating);
            Assert.Equal(expectedDistance, tourlog.TotalDistance);
            Assert.Equal(expectedTotalTime, tourlog.TotalTime);
        }

        [Fact]
        public void Tourlog_CanBeLinkedToTour()
        {
            var tour = new Tours(10, "Mountain Trip", "Scenic tour", "Innsbruck", "Salzburg", TimeSpan.FromHours(5), 120.5, TransportType.Bus);
            var tourlog = new Tourlog { Tour = tour };
            Assert.NotNull(tourlog.Tour);
            Assert.Equal("Mountain Trip", tourlog.Tour.Name);
        }
    }
}