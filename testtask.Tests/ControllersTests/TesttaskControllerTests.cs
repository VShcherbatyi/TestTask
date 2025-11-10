using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using testtask.DTOs;
using testtask.Controllers;
using testtask.Services.Interfaces;

public class TesttaskControllerTests
{
    [Fact]
    public Task Ping_Test()
    {
        // Arrange
        var mockService = new Mock<IDogService>();
        var controller = new TesttaskController(mockService.Object);

        // Act
        string result = controller.Ping();

        // Assert
        result.Should().BeEquivalentTo("Dogshouseservice.Version1.0.1");
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetDogsAsync_Dogs()
    {
        // Arrange  
        var expectedDogs = new List<DogDto>
        {
            new DogDto { Name = "Charlie", Color = "Black", TailLength = 15, Weight = 20 },
            new DogDto { Name = "Buddy", Color = "Brown", TailLength = 5, Weight = 25 },
            new DogDto { Name = "Rocky", Color = "White", TailLength = 10, Weight = 15 }
        };

        var mockService = new Mock<IDogService>();
        mockService
            .Setup(s => s.GetDogsAsync("", "", null, null))
            .ReturnsAsync(expectedDogs);

        var controller = new TesttaskController(mockService.Object);

        // Act
        var result = await controller.GetDogs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dogs = Assert.IsAssignableFrom<IEnumerable<DogDto>>(okResult.Value);
        Assert.Equal(expectedDogs, dogs);
    }

    [Fact]
    public async Task CreateDogAsync_CreatedDog()
    {
        // Arrange
        var mockService = new Mock<IDogService>();
        mockService
            .Setup(s => s.CreateDogAsync(It.IsAny<DogDto>()))
            .Returns(Task.CompletedTask);

        var controller = new TesttaskController(mockService.Object);

        var newDog = new DogDto
        {
            Name = "TestName",
            Color = "TestColor",
            TailLength = 10,
            Weight = 10
        };

        // Act
        var result = await controller.CreateDog(newDog);

        // Assert
        var CreatedResult = Assert.IsType<CreatedResult>(result);
        mockService.Verify(s => s.CreateDogAsync(It.Is<DogDto>(d => d.Name == "TestName")), Times.Once);
    }
}
