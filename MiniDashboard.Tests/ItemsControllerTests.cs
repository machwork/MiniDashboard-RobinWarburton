using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiniDashboard.Api.Controllers;
using MiniDashboard.Api.DTOs;
using MiniDashboard.Api.Services;
using Moq;
using Xunit;

namespace MiniDashboard.Tests;

public class ItemsControllerTests
{
    private readonly Mock<IItemService> _mockItemService;
    private readonly Mock<ILogger<ItemsController>> _mockLogger;
    private readonly ItemsController _controller;

    public ItemsControllerTests()
    {
        _mockItemService = new Mock<IItemService>();
        _mockLogger = new Mock<ILogger<ItemsController>>();
        _controller = new ItemsController(_mockItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetItems_ShouldReturnOkResult_WithListOfItems()
    {
        // Arrange
        var items = new List<ItemDto>
        {
            new() { Id = 1, Name = "Item 1", Description = "Description 1", Category = "Category 1", Price = 10.00m },
            new() { Id = 2, Name = "Item 2", Description = "Description 2", Category = "Category 2", Price = 20.00m }
        };

        _mockItemService.Setup(x => x.GetAllItemsAsync())
            .ReturnsAsync(items);

        // Act
        var result = await _controller.GetItems();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedItems = okResult.Value.Should().BeAssignableTo<IEnumerable<ItemDto>>().Subject;
        returnedItems.Should().HaveCount(2);
        _mockItemService.Verify(x => x.GetAllItemsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetItems_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockItemService.Setup(x => x.GetAllItemsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetItems();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetItem_ShouldReturnOkResult_WhenItemExists()
    {
        // Arrange
        var item = new ItemDto
        {
            Id = 1,
            Name = "Item 1",
            Description = "Description 1",
            Category = "Category 1",
            Price = 10.00m
        };

        _mockItemService.Setup(x => x.GetItemByIdAsync(1))
            .ReturnsAsync(item);

        // Act
        var result = await _controller.GetItem(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedItem = okResult.Value.Should().BeOfType<ItemDto>().Subject;
        returnedItem.Id.Should().Be(1);
        returnedItem.Name.Should().Be("Item 1");
        _mockItemService.Verify(x => x.GetItemByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        _mockItemService.Setup(x => x.GetItemByIdAsync(999))
            .ReturnsAsync((ItemDto?)null);

        // Act
        var result = await _controller.GetItem(999);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Item with ID 999 not found");
        _mockItemService.Verify(x => x.GetItemByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task SearchItems_ShouldReturnOkResult_WithSearchResults()
    {
        // Arrange
        var query = "test";
        var items = new List<ItemDto>
        {
            new() { Id = 1, Name = "Test Item", Description = "Description", Category = "Category", Price = 10.00m }
        };

        _mockItemService.Setup(x => x.SearchItemsAsync(query))
            .ReturnsAsync(items);

        // Act
        var result = await _controller.SearchItems(query);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedItems = okResult.Value.Should().BeAssignableTo<IEnumerable<ItemDto>>().Subject;
        returnedItems.Should().HaveCount(1);
        _mockItemService.Verify(x => x.SearchItemsAsync(query), Times.Once);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnCreatedResult_WhenItemIsValid()
    {
        // Arrange
        var createDto = new CreateItemDto
        {
            Name = "New Item",
            Description = "New Description",
            Category = "New Category",
            Price = 15.00m
        };

        var createdItem = new ItemDto
        {
            Id = 1,
            Name = "New Item",
            Description = "New Description",
            Category = "New Category",
            Price = 15.00m
        };

        _mockItemService.Setup(x => x.CreateItemAsync(createDto))
            .ReturnsAsync(createdItem);

        // Act
        var result = await _controller.CreateItem(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ItemsController.GetItem));
        createdResult.RouteValues.Should().NotBeNull();
        createdResult.RouteValues!["id"].Should().Be(1);
        var returnedItem = createdResult.Value.Should().BeOfType<ItemDto>().Subject;
        returnedItem.Name.Should().Be("New Item");
        _mockItemService.Verify(x => x.CreateItemAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var createDto = new CreateItemDto();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateItem(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        _mockItemService.Verify(x => x.CreateItemAsync(It.IsAny<CreateItemDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateItemsBulk_ShouldReturnCreatedResult_WhenItemsAreValid()
    {
        // Arrange
        var createDtos = new List<CreateItemDto>
        {
            new() { Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m },
            new() { Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m }
        };

        var createdItems = new List<ItemDto>
        {
            new() { Id = 1, Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m },
            new() { Id = 2, Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m }
        };

        _mockItemService.Setup(x => x.CreateItemsBulkAsync(createDtos))
            .ReturnsAsync(createdItems);

        // Act
        var result = await _controller.CreateItemsBulk(createDtos);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(201);
        var returnedItems = statusResult.Value.Should().BeAssignableTo<IEnumerable<ItemDto>>().Subject;
        returnedItems.Should().HaveCount(2);
        _mockItemService.Verify(x => x.CreateItemsBulkAsync(createDtos), Times.Once);
    }

    [Fact]
    public async Task CreateItemsBulk_ShouldReturnBadRequest_WhenListIsEmpty()
    {
        // Arrange
        var createDtos = new List<CreateItemDto>();

        // Act
        var result = await _controller.CreateItemsBulk(createDtos);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("At least one item is required");
        _mockItemService.Verify(x => x.CreateItemsBulkAsync(It.IsAny<IEnumerable<CreateItemDto>>()), Times.Never);
    }

    [Fact]
    public async Task CreateItemsBulk_ShouldReturnBadRequest_WhenListIsNull()
    {
        // Act
        var result = await _controller.CreateItemsBulk(null!);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be("At least one item is required");
        _mockItemService.Verify(x => x.CreateItemsBulkAsync(It.IsAny<IEnumerable<CreateItemDto>>()), Times.Never);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnOkResult_WhenItemExists()
    {
        // Arrange
        var updateDto = new UpdateItemDto
        {
            Name = "Updated Item",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 25.00m
        };

        var updatedItem = new ItemDto
        {
            Id = 1,
            Name = "Updated Item",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 25.00m
        };

        _mockItemService.Setup(x => x.UpdateItemAsync(1, updateDto))
            .ReturnsAsync(updatedItem);

        // Act
        var result = await _controller.UpdateItem(1, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedItem = okResult.Value.Should().BeOfType<ItemDto>().Subject;
        returnedItem.Name.Should().Be("Updated Item");
        _mockItemService.Verify(x => x.UpdateItemAsync(1, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateItemDto
        {
            Name = "Updated Item",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 25.00m
        };

        _mockItemService.Setup(x => x.UpdateItemAsync(999, updateDto))
            .ReturnsAsync((ItemDto?)null);

        // Act
        var result = await _controller.UpdateItem(999, updateDto);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Item with ID 999 not found");
        _mockItemService.Verify(x => x.UpdateItemAsync(999, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var updateDto = new UpdateItemDto();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdateItem(1, updateDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        _mockItemService.Verify(x => x.UpdateItemAsync(It.IsAny<int>(), It.IsAny<UpdateItemDto>()), Times.Never);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturnNoContent_WhenItemExists()
    {
        // Arrange
        _mockItemService.Setup(x => x.DeleteItemAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteItem(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockItemService.Verify(x => x.DeleteItemAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        _mockItemService.Setup(x => x.DeleteItemAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteItem(999);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be("Item with ID 999 not found");
        _mockItemService.Verify(x => x.DeleteItemAsync(999), Times.Once);
    }
}

