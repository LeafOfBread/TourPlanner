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
            string expectedName = "Mountain Trip";
            string expectedDescription = "A scenic tour in the Alps";
            string expectedFrom = "Innsbruck";
            string expectedTo = "Salzburg";
            string expectedImagePath = "images/mountain.jpg";
            TimeSpan expectedDuration = TimeSpan.FromHours(5);
            double expectedDistance = 120.5;

            // Act
            var tour = new Tours(expectedName, expectedDescription, expectedFrom, expectedTo, expectedImagePath, expectedDuration, expectedDistance);

            // Assert
            Assert.Equal(expectedName, tour.name);
            Assert.Equal(expectedDescription, tour.description);
            Assert.Equal(expectedFrom, tour.from);
            Assert.Equal(expectedTo, tour.to);
            Assert.Equal(expectedImagePath, tour.imagePath);
            Assert.Equal(expectedDuration, tour.duration);
            Assert.Equal(expectedDistance, tour.distance);
        }

            [Fact]
            public void Tourlog_Constructor_ShouldInitializeProperties()
            {
                // Arrange
                int expectedTourId = 1;
                int expectedTourLogId = 100;
                string expectedContent = "Amazing trip!";
                string expectedAuthor = "John Doe";

                // Act
                var tourlog = new Tourlog(expectedTourId, expectedTourLogId, expectedContent, expectedAuthor);

                // Assert
                Assert.Equal(expectedTourId, tourlog.TourId);
                Assert.Equal(expectedTourLogId, tourlog.TourLogId);
                Assert.Equal(expectedContent, tourlog.Content);
                Assert.Equal(expectedAuthor, tourlog.Author);
            }

        [Fact]
        public void Tourlog_CanBeLinkedToTour()
        {
            var tour = new Tours("Mountain Trip", "Scenic tour", "Innsbruck", "Salzburg", "image.jpg", TimeSpan.FromHours(5), 120.5);
            var tourlog = new Tourlog { Tour = tour };
            Assert.NotNull(tourlog.Tour);
            Assert.Equal("Mountain Trip", tourlog.Tour.name);
        }
    }
}