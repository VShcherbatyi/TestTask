using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using testtask.Data;
using testtask.DTOs;
using testtask.Models;
using testtask.Services.Implementations;

public class DogServiceTests
{
    private async Task<AppDbContext> GetInMemoryDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        context.Dogs.AddRange(
            new Dog { Name = "Charlie", Color = "Black", TailLength = 15, Weight = 20 },
            new Dog { Name = "Buddy", Color = "Brown", TailLength = 5, Weight = 25 },
            new Dog { Name = "Rocky", Color = "White", TailLength = 10, Weight = 15 }
        );

        await context.SaveChangesAsync();
        return context;
    }

    /* Endpoint: /dogs
     * Status: valid */

    [Fact]
    public async Task GetDogsAsync_SortedDogs1()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        var result = await service.GetDogsAsync("TailLength", "desc", null, null);

        // Assert
        var dogs = result.ToList();
        dogs.Should().HaveCount(3);
        dogs[0].Name.Should().Be("Charlie");
        dogs[1].Name.Should().Be("Rocky");
        dogs[2].Name.Should().Be("Buddy");
    }

    [Fact]
    public async Task GetDogsAsync_SortedDogs2()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        var result = await service.GetDogsAsync("weight", "asc", null, null);

        // Assert
        var dogs = result.ToList();
        dogs.Should().HaveCount(3);
        dogs[0].Name.Should().Be("Rocky");
        dogs[1].Name.Should().Be("Charlie");
        dogs[2].Name.Should().Be("Buddy");
    }

    [Fact]
    public async Task GetDogsAsync_PagedDogs1()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        var result = await service.GetDogsAsync("", "", 2, 1);

        // Assert
        var dogs = result.ToList();
        dogs.Should().HaveCount(1);
        dogs[0].Name.Should().Be("Buddy");
    }

    [Fact]
    public async Task GetDogsAsync_PagedDogs2()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        var result = await service.GetDogsAsync("", "", 2, 2);

        // Assert
        var dogs = result.ToList();
        dogs.Should().HaveCount(1);
        dogs[0].Name.Should().Be("Rocky");
    }

    [Fact]
    public async Task GetDogsAsync_SortedAndPagedDogs()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        var result = await service.GetDogsAsync("Weight", "desc", 2, 1);

        // Assert
        var dogs = result.ToList();
        dogs.Should().HaveCount(1);
        dogs[0].Name.Should().Be("Charlie");
    }

    /* Endpoint: /dogs
     * Status: invalid */

    [Fact]
    public async Task GetDogsAsync_WrongSortingInputs()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        Func<Task> act = async () => await service.GetDogsAsync("Weight", "", null, null);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Invalid sorting inputs*");
    }

    [Fact]
    public async Task GetDogsAsync_WrongSortingAttribute()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        Func<Task> act = async () => await service.GetDogsAsync("Height", "desc", null, null);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Invalid attribute name*")
            .Where(ex => ex.ParamName == "attribute");
    }

    [Fact]
    public async Task GetDogsAsync_WrongSortingOrder()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        Func<Task> act = async () => await service.GetDogsAsync("Weight", "sc", null, null);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Invalid order name*")
            .Where(ex => ex.ParamName == "order");
    }

    [Fact]
    public async Task GetDogsAsync_WrongPagingInputs()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        Func<Task> act = async () => await service.GetDogsAsync("", "", null, 1);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Invalid paging inputs*");
    }

    [Fact]
    public async Task GetDogsAsync_WrongPageNumber()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        Func<Task> act = async () => await service.GetDogsAsync("", "", 0, 1);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("Invalid pageNumber or/and pageSize*");
    }

    [Fact]
    public async Task GetDogsAsync_WrongPageSize()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        // Act
        Func<Task> act = async () => await service.GetDogsAsync("", "", 1, 0);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("Invalid pageNumber or/and pageSize*");
    }

    /* Endpoint: /dog
     * Status: invalid */

    [Fact]
    public async Task CreateDogAsync_WrongName()
    {
        // Arrange
        var context = await GetInMemoryDbContextAsync();
        var service = new DogService(context);

        var newDog = new DogDto
        {
            Name = "Charlie",
            Color = "Pink",
            TailLength = 10,
            Weight = 10
        };

        // Act
        Func<Task> act = async () => await service.CreateDogAsync(newDog);

        // Assert
        await act.Should()
            .ThrowAsync<DuplicateNameException>()
            .WithMessage("Name is already taken");
    }
}
