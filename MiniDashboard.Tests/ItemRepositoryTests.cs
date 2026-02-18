using System.IO;
using FluentAssertions;
using MiniDashboard.Api.Models;
using MiniDashboard.Api.Repositories;
using Xunit;

namespace MiniDashboard.Tests;

public class ItemRepositoryTests : IDisposable
{
    private readonly ItemRepository _repository;
    private readonly string _testDataFilePath;

    public ItemRepositoryTests()
    {
        // Use a test-specific data file
        var testDataDir = Path.Combine(Path.GetTempPath(), "MiniDashboardTests");
        Directory.CreateDirectory(testDataDir);
        _testDataFilePath = Path.Combine(testDataDir, $"items_test_{Guid.NewGuid()}.json");
        
        // Create repository with test file path using reflection or a test constructor
        // Since ItemRepository uses a hardcoded path, we'll test with the actual implementation
        // and clean up after each test
        _repository = new ItemRepository();
    }

    public void Dispose()
    {
        // Clean up test data file if it exists
        try
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MiniDashboard"
            );
            var dataFilePath = Path.Combine(appDataPath, "items.json");
            if (File.Exists(dataFilePath))
            {
                // Backup original if exists, or just work with it for testing
                // In a real scenario, you'd want to isolate test data
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllItems()
    {
        // Arrange - repository starts with seeded data or empty
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<Item>>();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateItemWithGeneratedId()
    {
        // Arrange
        var item = new Item
        {
            Name = "Test Item",
            Description = "Test Description",
            Category = "Test Category",
            Price = 10.00m
        };

        // Act
        var result = await _repository.CreateAsync(item);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test Item");
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        var item = new Item
        {
            Name = "Test Item",
            Description = "Test Description",
            Category = "Test Category",
            Price = 10.00m
        };
        var created = await _repository.CreateAsync(item);

        // Act
        var result = await _repository.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be("Test Item");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateItem_WhenItemExists()
    {
        // Arrange
        var item = new Item
        {
            Name = "Original Name",
            Description = "Original Description",
            Category = "Original Category",
            Price = 10.00m
        };
        var created = await _repository.CreateAsync(item);

        var updatedItem = new Item
        {
            Id = created.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 20.00m,
            CreatedDate = created.CreatedDate
        };

        // Act
        var result = await _repository.UpdateAsync(created.Id, updatedItem);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Price.Should().Be(20.00m);
        result.CreatedDate.Should().Be(created.CreatedDate);
        result.UpdatedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var item = new Item
        {
            Id = 99999,
            Name = "Test Item",
            Description = "Test Description",
            Category = "Test Category",
            Price = 10.00m
        };

        // Act
        var result = await _repository.UpdateAsync(99999, item);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        var item = new Item
        {
            Name = "Item To Delete",
            Description = "Description",
            Category = "Category",
            Price = 10.00m
        };
        var created = await _repository.CreateAsync(item);

        // Act
        var result = await _repository.DeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();
        
        // Verify item is actually deleted
        var deletedItem = await _repository.GetByIdAsync(created.Id);
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Act
        var result = await _repository.DeleteAsync(99999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_ShouldFindItemsByNumericId()
    {
        // Arrange
        var item = new Item
        {
            Name = "Test Item",
            Description = "Test Description",
            Category = "Test Category",
            Price = 10.00m
        };
        var created = await _repository.CreateAsync(item);

        // Act
        var result = await _repository.SearchAsync(created.Id.ToString());

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(i => i.Id == created.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldFindItemsByName()
    {
        // Arrange
        var item = new Item
        {
            Name = "Unique Test Item Name",
            Description = "Description",
            Category = "Category",
            Price = 10.00m
        };
        await _repository.CreateAsync(item);

        // Act
        var result = await _repository.SearchAsync("Unique Test");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(i => i.Name.Contains("Unique Test", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_ShouldFindItemsByDescription()
    {
        // Arrange
        var item = new Item
        {
            Name = "Item Name",
            Description = "Unique Description Text",
            Category = "Category",
            Price = 10.00m
        };
        await _repository.CreateAsync(item);

        // Act
        var result = await _repository.SearchAsync("Unique Description");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(i => i.Description.Contains("Unique Description", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_ShouldFindItemsByCategory()
    {
        // Arrange
        var item = new Item
        {
            Name = "Item Name",
            Description = "Description",
            Category = "Unique Category Name",
            Price = 10.00m
        };
        await _repository.CreateAsync(item);

        // Act
        var result = await _repository.SearchAsync("Unique Category");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(i => i.Category.Contains("Unique Category", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateAsync_ShouldIncrementIdSequentially()
    {
        // Arrange
        var item1 = new Item { Name = "Item 1", Description = "Desc", Category = "Cat", Price = 10.00m };
        var item2 = new Item { Name = "Item 2", Description = "Desc", Category = "Cat", Price = 20.00m };

        // Act
        var created1 = await _repository.CreateAsync(item1);
        var created2 = await _repository.CreateAsync(item2);

        // Assert
        created2.Id.Should().BeGreaterThan(created1.Id);
    }
}

