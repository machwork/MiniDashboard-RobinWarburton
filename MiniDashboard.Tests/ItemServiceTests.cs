using FluentAssertions;
using MiniDashboard.Api.DTOs;
using MiniDashboard.Api.Models;
using MiniDashboard.Api.Repositories;
using MiniDashboard.Api.Services;
using Moq;
using Xunit;

namespace MiniDashboard.Tests;

public class ItemServiceTests
{
    private readonly Mock<IItemRepository> _mockRepository;
    private readonly ItemService _service;

    public ItemServiceTests()
    {
        _mockRepository = new Mock<IItemRepository>();
        _service = new ItemService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllItemsAsync_ShouldReturnAllItemsAsDtos()
    {
        // Arrange
        var items = new List<Item>
        {
            new() { Id = 1, Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m, CreatedDate = DateTime.Now },
            new() { Id = 2, Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m, CreatedDate = DateTime.Now }
        };

        _mockRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(items);

        // Act
        var result = await _service.GetAllItemsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<ItemDto>();
        result.First().Id.Should().Be(1);
        result.First().Name.Should().Be("Item 1");
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetItemByIdAsync_ShouldReturnItemDto_WhenItemExists()
    {
        // Arrange
        var item = new Item
        {
            Id = 1,
            Name = "Item 1",
            Description = "Desc 1",
            Category = "Cat 1",
            Price = 10.00m,
            CreatedDate = DateTime.Now
        };

        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(item);

        // Act
        var result = await _service.GetItemByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Item 1");
        result.Price.Should().Be(10.00m);
        _mockRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetItemByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Item?)null);

        // Act
        var result = await _service.GetItemByIdAsync(999);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(x => x.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task SearchItemsAsync_ShouldReturnMatchingItemsAsDtos()
    {
        // Arrange
        var query = "test";
        var items = new List<Item>
        {
            new() { Id = 1, Name = "Test Item", Description = "Description", Category = "Category", Price = 10.00m, CreatedDate = DateTime.Now }
        };

        _mockRepository.Setup(x => x.SearchAsync(query))
            .ReturnsAsync(items);

        // Act
        var result = await _service.SearchItemsAsync(query);

        // Assert
        result.Should().HaveCount(1);
        result.Should().AllBeOfType<ItemDto>();
        result.First().Name.Should().Contain("Test");
        _mockRepository.Verify(x => x.SearchAsync(query), Times.Once);
    }

    [Fact]
    public async Task CreateItemAsync_ShouldCreateItemAndReturnDto()
    {
        // Arrange
        var createDto = new CreateItemDto
        {
            Name = "New Item",
            Description = "New Description",
            Category = "New Category",
            Price = 15.00m
        };

        var createdItem = new Item
        {
            Id = 1,
            Name = "New Item",
            Description = "New Description",
            Category = "New Category",
            Price = 15.00m,
            CreatedDate = DateTime.Now
        };

        _mockRepository.Setup(x => x.CreateAsync(It.Is<Item>(i => 
            i.Name == createDto.Name &&
            i.Description == createDto.Description &&
            i.Category == createDto.Category &&
            i.Price == createDto.Price)))
            .ReturnsAsync(createdItem);

        // Act
        var result = await _service.CreateItemAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("New Item");
        result.Price.Should().Be(15.00m);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public async Task CreateItemsBulkAsync_ShouldCreateMultipleItemsAndReturnDtos()
    {
        // Arrange
        var createDtos = new List<CreateItemDto>
        {
            new() { Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m },
            new() { Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m }
        };

        var createdItems = new List<Item>
        {
            new() { Id = 1, Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m, CreatedDate = DateTime.Now },
            new() { Id = 2, Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m, CreatedDate = DateTime.Now }
        };

        _mockRepository.SetupSequence(x => x.CreateAsync(It.IsAny<Item>()))
            .ReturnsAsync(createdItems[0])
            .ReturnsAsync(createdItems[1]);

        // Act
        var result = await _service.CreateItemsBulkAsync(createDtos);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<ItemDto>();
        result.First().Name.Should().Be("Item 1");
        result.Last().Name.Should().Be("Item 2");
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Item>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldUpdateItemAndReturnDto_WhenItemExists()
    {
        // Arrange
        var existingItem = new Item
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description",
            Category = "Old Category",
            Price = 10.00m,
            CreatedDate = DateTime.Now.AddDays(-1)
        };

        var updateDto = new UpdateItemDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 25.00m
        };

        var updatedItem = new Item
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 25.00m,
            CreatedDate = existingItem.CreatedDate,
            UpdatedDate = DateTime.Now
        };

        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingItem);

        _mockRepository.Setup(x => x.UpdateAsync(1, It.Is<Item>(i => 
            i.Id == 1 &&
            i.Name == updateDto.Name &&
            i.Price == updateDto.Price &&
            i.CreatedDate == existingItem.CreatedDate)))
            .ReturnsAsync(updatedItem);

        // Act
        var result = await _service.UpdateItemAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Updated Name");
        result.Price.Should().Be(25.00m);
        result.CreatedDate.Should().Be(existingItem.CreatedDate);
        _mockRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(1, It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateItemDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 25.00m
        };

        _mockRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Item?)null);

        // Act
        var result = await _service.UpdateItemAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(x => x.GetByIdAsync(999), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task DeleteItemAsync_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        _mockRepository.Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteItemAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteItemAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(x => x.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteItemAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(x => x.DeleteAsync(999), Times.Once);
    }
}

