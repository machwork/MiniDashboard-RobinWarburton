using System.Text.Json;
using MiniDashboard.Api.Models;

namespace MiniDashboard.Api.Repositories;

/// <summary>
/// File-based repository implementation for Item data access with JSON persistence
/// </summary>
public class ItemRepository : IItemRepository
{
    private readonly List<Item> _items = new();
    private int _nextId = 1;
    private readonly object _lock = new();
    private readonly string _dataFilePath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ItemRepository()
    {
        // Store data file in user's AppData\Local\MiniDashboard folder
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniDashboard"
        );
        Directory.CreateDirectory(appDataPath);
        _dataFilePath = Path.Combine(appDataPath, "items.json");
        
        // Load existing data or seed with initial data
        LoadData();
    }

    public Task<IEnumerable<Item>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Item>>(_items.ToList());
        }
    }

    public Task<Item?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            return Task.FromResult(item);
        }
    }

    public Task<IEnumerable<Item>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetAllAsync();
        }

        lock (_lock)
        {
            var lowerQuery = query.ToLowerInvariant();
            
            // Check if query is a numeric ID
            if (int.TryParse(query, out int id))
            {
                var itemById = _items.FirstOrDefault(i => i.Id == id);
                if (itemById != null)
                {
                    return Task.FromResult<IEnumerable<Item>>(new[] { itemById });
                }
            }
            
            // Search by text fields
            var results = _items.Where(i =>
                i.Name.ToLowerInvariant().Contains(lowerQuery) ||
                i.Description.ToLowerInvariant().Contains(lowerQuery) ||
                i.Category.ToLowerInvariant().Contains(lowerQuery)
            ).ToList();

            return Task.FromResult<IEnumerable<Item>>(results);
        }
    }

    public Task<Item> CreateAsync(Item item)
    {
        lock (_lock)
        {
            item.Id = _nextId++;
            item.CreatedDate = DateTime.UtcNow;
            _items.Add(item);
            SaveData();
            return Task.FromResult(item);
        }
    }

    public Task<Item?> UpdateAsync(int id, Item item)
    {
        lock (_lock)
        {
            var existingItem = _items.FirstOrDefault(i => i.Id == id);
            if (existingItem == null)
            {
                return Task.FromResult<Item?>(null);
            }

            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.Category = item.Category;
            existingItem.Price = item.Price;
            existingItem.UpdatedDate = DateTime.UtcNow;
            SaveData();

            return Task.FromResult<Item?>(existingItem);
        }
    }

    public Task<bool> DeleteAsync(int id)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item == null)
            {
                return Task.FromResult(false);
            }

            _items.Remove(item);
            SaveData();
            return Task.FromResult(true);
        }
    }

    private void LoadData()
    {
        lock (_lock)
        {
            if (File.Exists(_dataFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var loadedItems = JsonSerializer.Deserialize<List<Item>>(json, _jsonOptions);
                    if (loadedItems != null && loadedItems.Count > 0)
                    {
                        _items.Clear();
                        _items.AddRange(loadedItems);
                        _nextId = _items.Count > 0 ? _items.Max(i => i.Id) + 1 : 1;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Log error and fall back to seed data
                    Console.WriteLine($"Error loading data file: {ex.Message}");
                }
            }
            
            // If file doesn't exist or loading failed, seed with initial data
            SeedData();
            SaveData();
        }
    }

    private void SaveData()
    {
        try
        {
            var json = JsonSerializer.Serialize(_items, _jsonOptions);
            File.WriteAllText(_dataFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving data file: {ex.Message}");
        }
    }

    private void SeedData()
    {
        var seedItems = new List<Item>
        {
            new Item
            {
                Id = _nextId++,
                Name = "Introduction to C#",
                Description = "A comprehensive guide to C# programming",
                Category = "Books",
                Price = 29.99m,
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new Item
            {
                Id = _nextId++,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with long battery life",
                Category = "Devices",
                Price = 24.99m,
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            },
            new Item
            {
                Id = _nextId++,
                Name = "Mechanical Keyboard",
                Description = "RGB backlit mechanical keyboard",
                Category = "Devices",
                Price = 89.99m,
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            },
            new Item
            {
                Id = _nextId++,
                Name = "Clean Code",
                Description = "A Handbook of Agile Software Craftsmanship",
                Category = "Books",
                Price = 35.00m,
                CreatedDate = DateTime.UtcNow.AddDays(-7)
            },
            new Item
            {
                Id = _nextId++,
                Name = "USB-C Hub",
                Description = "Multi-port USB-C hub with HDMI and SD card reader",
                Category = "Devices",
                Price = 45.99m,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            }
        };

        _items.AddRange(seedItems);
    }
}


