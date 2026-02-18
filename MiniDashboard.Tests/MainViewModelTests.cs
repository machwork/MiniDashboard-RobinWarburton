using FluentAssertions;
using MiniDashboard.App.Commands;
using MiniDashboard.App.Models;
using MiniDashboard.App.Services;
using MiniDashboard.App.ViewModels;
using Moq;
using Xunit;

namespace MiniDashboard.Tests;

public class MainViewModelTests
{
    private readonly Mock<IApiService> _mockApiService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mockApiService = new Mock<IApiService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new MainViewModel(_mockApiService.Object, _mockNavigationService.Object);
    }

    [Fact]
    public async Task LoadItemsAsync_ShouldLoadItems_WhenApiReturnsItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new() { Id = 1, Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m, CreatedDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m, CreatedDate = DateTime.UtcNow }
        };
        _mockApiService.Setup(x => x.GetItemsAsync()).ReturnsAsync(items);

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        _viewModel.Items.Should().HaveCount(2);
        _viewModel.ItemsCount.Should().Be(2);
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadItemsAsync_ShouldSetErrorMessage_WhenApiThrowsException()
    {
        // Arrange
        _mockApiService.Setup(x => x.GetItemsAsync()).ThrowsAsync(new Exception("API Error"));

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        _viewModel.ErrorMessage.Should().Contain("API Error");
        _viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task SearchItemsAsync_ShouldFilterItems_WhenSearchQueryIsProvided()
    {
        // Arrange
        var allItems = new List<Item>
        {
            new() { Id = 1, Name = "Item 1", Description = "Desc 1", Category = "Cat 1", Price = 10.00m, CreatedDate = DateTime.UtcNow },
            new() { Id = 2, Name = "Item 2", Description = "Desc 2", Category = "Cat 2", Price = 20.00m, CreatedDate = DateTime.UtcNow }
        };
        var searchResults = new List<Item> { allItems[0] };
        _mockApiService.Setup(x => x.GetItemsAsync()).ReturnsAsync(allItems);
        _mockApiService.Setup(x => x.SearchItemsAsync("Item 1")).ReturnsAsync(searchResults);
        await _viewModel.LoadItemsAsync();

        // Act
        _viewModel.SearchQuery = "Item 1";
        if (_viewModel.SearchCommand is AsyncRelayCommand searchCmd)
        {
            await searchCmd.ExecuteAsync(null);
        }

        // Assert
        _viewModel.Items.Should().HaveCount(1);
        _viewModel.Items[0].Name.Should().Be("Item 1");
    }

    [Fact]
    public void AddNewItem_ShouldCreateNewItem_AndSetAsSelected()
    {
        // Act
        _viewModel.AddItemCommand.Execute(null);

        // Assert
        _viewModel.SelectedItem.Should().NotBeNull();
        _viewModel.SelectedItem!.Id.Should().Be(0);
        _viewModel.SelectedItem.Name.Should().BeEmpty();
    }

    [Fact]
    public void EditSelectedItem_ShouldNotChange_WhenNoItemSelected()
    {
        // Arrange
        _viewModel.SelectedItem = null;

        // Act
        _viewModel.EditItemCommand.Execute(null);

        // Assert
        _viewModel.SelectedItem.Should().BeNull();
    }

    [Fact]
    public async Task SaveItemAsync_ShouldCreateItem_WhenItemIsNew()
    {
        // Arrange
        var newItem = new Item { Id = 0, Name = "New Item", Description = "Desc", Category = "Cat", Price = 10.00m };
        var createdItem = new Item { Id = 1, Name = "New Item", Description = "Desc", Category = "Cat", Price = 10.00m, CreatedDate = DateTime.UtcNow };
        _viewModel.SelectedItem = newItem;
        _mockApiService.Setup(x => x.CreateItemAsync(It.IsAny<Item>())).ReturnsAsync(createdItem);
        _mockApiService.Setup(x => x.GetItemsAsync()).ReturnsAsync(new List<Item> { createdItem });

        // Act
        if (_viewModel.SaveItemCommand is AsyncRelayCommand saveCmd)
        {
            await saveCmd.ExecuteAsync(null);
        }

        // Assert
        _mockApiService.Verify(x => x.CreateItemAsync(It.IsAny<Item>()), Times.Once);
        _viewModel.SelectedItem.Should().BeNull();
    }

    [Fact]
    public async Task SaveItemAsync_ShouldUpdateItem_WhenItemExists()
    {
        // Arrange
        var existingItem = new Item { Id = 1, Name = "Item", Description = "Desc", Category = "Cat", Price = 10.00m, CreatedDate = DateTime.UtcNow };
        var updatedItem = new Item { Id = 1, Name = "Updated Item", Description = "Desc", Category = "Cat", Price = 10.00m, CreatedDate = DateTime.UtcNow };
        _viewModel.SelectedItem = existingItem;
        _mockApiService.Setup(x => x.UpdateItemAsync(1, It.IsAny<Item>())).ReturnsAsync(updatedItem);
        _mockApiService.Setup(x => x.GetItemsAsync()).ReturnsAsync(new List<Item> { updatedItem });

        // Act
        if (_viewModel.SaveItemCommand is AsyncRelayCommand saveCmd)
        {
            await saveCmd.ExecuteAsync(null);
        }

        // Assert
        _mockApiService.Verify(x => x.UpdateItemAsync(1, It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSelectedItemAsync_ShouldDeleteItem_WhenItemIsSelected()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Item", Description = "Desc", Category = "Cat", Price = 10.00m, CreatedDate = DateTime.UtcNow };
        _viewModel.SelectedItem = item;
        _mockApiService.Setup(x => x.DeleteItemAsync(1)).ReturnsAsync(true);
        _mockApiService.Setup(x => x.GetItemsAsync()).ReturnsAsync(new List<Item>());

        // Act
        if (_viewModel.DeleteItemCommand is AsyncRelayCommand deleteCmd)
        {
            await deleteCmd.ExecuteAsync(null);
        }

        // Assert
        _mockApiService.Verify(x => x.DeleteItemAsync(1), Times.Once);
        _viewModel.SelectedItem.Should().BeNull();
    }

    [Fact]
    public async Task SortItems_ShouldSortItems_WhenColumnIsProvided()
    {
        // Arrange
        var items = new List<Item>
        {
            new() { Id = 2, Name = "B", Description = "Desc", Category = "Cat", Price = 20.00m, CreatedDate = DateTime.UtcNow },
            new() { Id = 1, Name = "A", Description = "Desc", Category = "Cat", Price = 10.00m, CreatedDate = DateTime.UtcNow }
        };
        _mockApiService.Setup(x => x.GetItemsAsync()).ReturnsAsync(items);
        await _viewModel.LoadItemsAsync();

        // Act
        _viewModel.SortCommand.Execute("Name");

        // Assert
        _viewModel.Items[0].Name.Should().Be("A");
        _viewModel.Items[1].Name.Should().Be("B");
    }

    [Fact]
    public async Task GoToNextPage_ShouldIncrementCurrentPage_WhenNotOnLastPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).Select(i => new Item 
        { 
            Id = i, 
            Name = $"Item {i}", 
            Description = "Desc", 
            Category = "Cat", 
            Price = 10.00m, 
            CreatedDate = DateTime.UtcNow 
        }).ToList();
        _mockApiService.Setup(x => x.GetItemsAsync()).ReturnsAsync(items);
        await _viewModel.LoadItemsAsync();
        _viewModel.CurrentPage.Should().Be(1);

        // Act
        _viewModel.NextPageCommand.Execute(null);

        // Assert
        _viewModel.CurrentPage.Should().Be(2);
    }

    [Fact]
    public void ClearError_ShouldClearErrorMessage()
    {
        // Arrange
        _viewModel.ErrorMessage = "Test Error";

        // Act
        _viewModel.ClearErrorCommand.Execute(null);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
    }
}

