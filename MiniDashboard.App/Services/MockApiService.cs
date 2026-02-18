using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MiniDashboard.App.Models;

namespace MiniDashboard.App.Services;

/// <summary>
/// Simple in-memory mock implementation of IApiService for development.
/// </summary>
public class MockApiService : IApiService
{
    private readonly List<Item> _items;
    private int _nextId;
    private readonly ILogger<MockApiService> _logger;

    public MockApiService(ILogger<MockApiService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _items = new List<Item>
        {
            new Item { Id = 1, Name = "Sample Item 1", Description = "A mock item", Category = "General", Price = 9.99M, CreatedDate = DateTime.UtcNow },
            new Item { Id = 2, Name = "Sample Item 2", Description = "Another mock item", Category = "Tools", Price = 19.5M, CreatedDate = DateTime.UtcNow.AddDays(-1) }
        };
        _nextId = _items.Max(i => i.Id) + 1;
        _logger.LogInformation("MockApiService initialized with {Count} items", _items.Count);
    }

    public Task<IEnumerable<Item>> GetItemsAsync()
    {
        _logger.LogDebug("Returning {Count} mock items", _items.Count);
        return Task.FromResult(_items.AsEnumerable());
    }

    public Task<Item?> GetItemByIdAsync(int id)
    {
        _logger.LogDebug("GetItemByIdAsync called for id {Id}", id);
        var item = _items.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(item);
    }

    public Task<IEnumerable<Item>> SearchItemsAsync(string query)
    {
        _logger.LogDebug("SearchItemsAsync called with query '{Query}'", query);
        var q = (query ?? string.Empty).Trim();
        var result = _items.Where(i => i.Name.Contains(q, System.StringComparison.OrdinalIgnoreCase) || i.Description.Contains(q, System.StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(result);
    }

    public Task<Item> CreateItemAsync(Item item)
    {
        _logger.LogInformation("Creating mock item {Name}", item.Name);
        var newItem = new Item
        {
            Id = _nextId++,
            Name = item.Name,
            Description = item.Description,
            Category = item.Category,
            Price = item.Price,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = null
        };
        _items.Add(newItem);
        return Task.FromResult(newItem);
    }

    public Task<Item?> UpdateItemAsync(int id, Item item)
    {
        _logger.LogInformation("Updating mock item {Id}", id);
        var existing = _items.FirstOrDefault(i => i.Id == id);
        if (existing == null) return Task.FromResult<Item?>(null);
        existing.Name = item.Name;
        existing.Description = item.Description;
        existing.Category = item.Category;
        existing.Price = item.Price;
        existing.UpdatedDate = DateTime.UtcNow;
        _logger.LogDebug("Mock item {Id} updated", id);
        return Task.FromResult<Item?>(existing);
    }

    public Task<bool> DeleteItemAsync(int id)
    {
        _logger.LogInformation("Deleting mock item {Id}", id);
        var existing = _items.FirstOrDefault(i => i.Id == id);
        if (existing == null) return Task.FromResult(false);
        _items.Remove(existing);
        return Task.FromResult(true);
    }
}
