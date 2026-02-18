using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using FluentAssertions;
using MiniDashboard.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MiniDashboard.Tests;

// Create a custom WebApplicationFactory for top-level Program.cs
// Since Program.cs uses top-level statements, we reference the assembly
public class CustomWebApplicationFactory : WebApplicationFactory<MiniDashboard.Api.Program>
{
}

public class ItemsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ItemsApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetItems_ShouldReturnOk_WithListOfItems()
    {
        // Act
        var response = await _client.GetAsync("/api/Items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetItem_ShouldReturnOk_WhenItemExists()
    {
        // Arrange - Create an item first
        var createDto = new CreateItemDto
        {
            Name = "Test Item",
            Description = "Test Description",
            Category = "Test Category",
            Price = 10.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Items", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemDto>();

        // Act
        var response = await _client.GetAsync($"/api/Items/{createdItem!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<ItemDto>();
        item.Should().NotBeNull();
        item!.Name.Should().Be("Test Item");
    }

    [Fact]
    public async Task GetItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/Items/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnCreated_WhenItemIsValid()
    {
        // Arrange
        var createDto = new CreateItemDto
        {
            Name = "New Item",
            Description = "New Description",
            Category = "New Category",
            Price = 15.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Items", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await response.Content.ReadFromJsonAsync<ItemDto>();
        item.Should().NotBeNull();
        item!.Name.Should().Be("New Item");
        item.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnBadRequest_WhenItemIsInvalid()
    {
        // Arrange
        var createDto = new CreateItemDto
        {
            Name = "", // Invalid: empty name
            Description = "Description",
            Category = "Category",
            Price = -10.00m // Invalid: negative price
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Items", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnOk_WhenItemExists()
    {
        // Arrange - Create an item first
        var createDto = new CreateItemDto
        {
            Name = "Original Item",
            Description = "Original Description",
            Category = "Category",
            Price = 10.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Items", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemDto>();

        var updateDto = new UpdateItemDto
        {
            Name = "Updated Item",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 20.00m
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Items/{createdItem!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedItem = await response.Content.ReadFromJsonAsync<ItemDto>();
        updatedItem.Should().NotBeNull();
        updatedItem!.Name.Should().Be("Updated Item");
        updatedItem.Price.Should().Be(20.00m);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateItemDto
        {
            Name = "Updated Item",
            Description = "Description",
            Category = "Category",
            Price = 10.00m
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/Items/99999", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturnNoContent_WhenItemExists()
    {
        // Arrange - Create an item first
        var createDto = new CreateItemDto
        {
            Name = "Item To Delete",
            Description = "Description",
            Category = "Category",
            Price = 10.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Items", createDto);
        createResponse.EnsureSuccessStatusCode();
        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/Items/{createdItem!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify item is deleted
        var getResponse = await _client.GetAsync($"/api/Items/{createdItem.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/Items/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchItems_ShouldReturnOk_WithMatchingItems()
    {
        // Arrange - Create items first
        var createDto1 = new CreateItemDto { Name = "Apple", Description = "Red fruit", Category = "Fruit", Price = 1.00m };
        var createDto2 = new CreateItemDto { Name = "Banana", Description = "Yellow fruit", Category = "Fruit", Price = 1.50m };
        await _client.PostAsJsonAsync("/api/Items", createDto1);
        await _client.PostAsJsonAsync("/api/Items", createDto2);

        // Act
        var response = await _client.GetAsync("/api/Items/search?query=Apple");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ItemDto>>();
        items.Should().NotBeNull();
        items!.Should().Contain(i => i.Name.Contains("Apple", StringComparison.OrdinalIgnoreCase));
    }
}

