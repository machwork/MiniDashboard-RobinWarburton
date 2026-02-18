using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MiniDashboard.App.Models;
using MiniDashboard.App.Services;
using Moq;
using Moq.Protected;
using Xunit;

namespace MiniDashboard.Tests;

public class ApiServiceTests
{
    private readonly Mock<ILogger<ApiService>> _mockLogger;

    public ApiServiceTests()
    {
        _mockLogger = new Mock<ILogger<ApiService>>();
    }

    [Fact]
    public async Task GetItemsAsync_ShouldReturnItems_WhenApiReturnsSuccess()
    {
        // Arrange
        var items = new List<Item>
        {
            new() { Id = 1, Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m, CreatedDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m, CreatedDate = DateTime.UtcNow }
        };
        var json = JsonSerializer.Serialize(items);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost:7044/") };
        var apiService = new ApiService(httpClient, _mockLogger.Object);

        // Act
        var result = await apiService.GetItemsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Item 1");
    }

    [Fact]
    public async Task GetItemsAsync_ShouldThrowException_WhenApiReturnsError()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost:7044/") };
        var apiService = new ApiService(httpClient, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await apiService.GetItemsAsync());
    }

    [Fact]
    public async Task CreateItemAsync_ShouldReturnCreatedItem_WhenApiReturnsSuccess()
    {
        // Arrange
        var item = new Item { Id = 0, Name = "New Item", Description = "Desc", Category = "Cat", Price = 10.00m };
        var createdItem = new Item { Id = 1, Name = "New Item", Description = "Desc", Category = "Cat", Price = 10.00m, CreatedDate = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(createdItem);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost:7044/") };
        var apiService = new ApiService(httpClient, _mockLogger.Object);

        // Act
        var result = await apiService.CreateItemAsync(item);

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("New Item");
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldReturnUpdatedItem_WhenItemExists()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Updated Item", Description = "Desc", Category = "Cat", Price = 10.00m, CreatedDate = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(item);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost:7044/") };
        var apiService = new ApiService(httpClient, _mockLogger.Object);

        // Act
        var result = await apiService.UpdateItemAsync(1, item);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Item");
    }

    [Fact]
    public async Task DeleteItemAsync_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost:7044/") };
        var apiService = new ApiService(httpClient, _mockLogger.Object);

        // Act
        var result = await apiService.DeleteItemAsync(1);

        // Assert
        result.Should().BeTrue();
    }
}

