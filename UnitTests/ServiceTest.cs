using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Moq;
using TourPlanner.ViewModels;
using TourPlannerClasses.Models;
using TourPlannerClasses.Services;
using TourPlannerClasses.Tour;
using Xunit;

public class ServiceTest
{
    [Fact]
    public async Task GetAllToursAsync_ShouldLoadToursCorrectly()
    {
        // Arrange: Create a mock TourService
        var mockTourService = new Mock<TourService>();

        // Sample data to return
        var mockTours = new List<Tours>
        {
            new Tours(1, "Tour 1", "Description 1", "Start 1", "End 1", new System.TimeSpan(1, 0, 0), 10.5, TransportType.Boat),
            new Tours(2, "Tour 2", "Description 2", "Start 2", "End 2", new System.TimeSpan(2, 0, 0), 20.0, TransportType.Bus)
        };

        // Set up the mock to return the mockTours when GetAllTours() is called
        mockTourService.Setup(s => s.GetAllTours()).ReturnsAsync(mockTours);

        // Create an instance of TourViewModel with the mock service
        var viewModel = new TourViewModel(mockTourService.Object, null);

        // Act: Call the method
        await Task.Delay(500);

        // Assert: Check if AllTours is updated correctly
        Assert.NotNull(viewModel.AllTours);
        Assert.Equal(2, viewModel.AllTours.Count);
        Assert.Equal("Tour 1", viewModel.AllTours[0].Name);
        Assert.Equal("Tour 2", viewModel.AllTours[1].Name);
    }
}
