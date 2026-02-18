using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using MiniDashboard.App.Models;

namespace MiniDashboard.App.Services;

/// <summary>
/// Implementation of cache service with in-memory and file-based persistence
/// </summary>
public class CacheService : ICacheService
{
    private readonly ConcurrentDictionary<int, Item> _inMemoryCache = new();
    private readonly string _cacheFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private DateTime? _cacheLastUpdated;
    private readonly object _lock = new();

    public CacheService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MiniDashboard"
        );
        Directory.CreateDirectory(appDataPath);
        _cacheFilePath = Path.Combine(appDataPath, "items_cache.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // Load cache from file on startup
        LoadCacheFromFile();
    }

    public Task<IEnumerable<Item>> GetCachedItemsAsync()
    {
        return Task.FromResult<IEnumerable<Item>>(_inMemoryCache.Values.ToList());
    }

    public Task<Item?> GetCachedItemByIdAsync(int id)
    {
        _inMemoryCache.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task<IEnumerable<Item>> SearchCachedItemsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return GetCachedItemsAsync();
        }

        var lowerQuery = query.ToLowerInvariant();
        var results = _inMemoryCache.Values.Where(item =>
            item.Name.ToLowerInvariant().Contains(lowerQuery) ||
            item.Description.ToLowerInvariant().Contains(lowerQuery) ||
            item.Category.ToLowerInvariant().Contains(lowerQuery) ||
            item.Id.ToString().Contains(lowerQuery)
        ).ToList();

        return Task.FromResult<IEnumerable<Item>>(results);
    }

    public async Task SaveItemsToCacheAsync(IEnumerable<Item> items)
    {
        lock (_lock)
        {
            _inMemoryCache.Clear();
            foreach (var item in items)
            {
                _inMemoryCache[item.Id] = item;
            }
            _cacheLastUpdated = DateTime.UtcNow;
        }

        await SaveCacheToFileAsync();
    }

    public async Task SaveItemToCacheAsync(Item item)
    {
        lock (_lock)
        {
            _inMemoryCache[item.Id] = item;
            _cacheLastUpdated = DateTime.UtcNow;
        }

        await SaveCacheToFileAsync();
    }

    public async Task RemoveItemFromCacheAsync(int id)
    {
        lock (_lock)
        {
            _inMemoryCache.TryRemove(id, out _);
            _cacheLastUpdated = DateTime.UtcNow;
        }

        await SaveCacheToFileAsync();
    }

    public async Task ClearCacheAsync()
    {
        lock (_lock)
        {
            _inMemoryCache.Clear();
            _cacheLastUpdated = null;
        }

        await SaveCacheToFileAsync();
    }

    public bool IsCacheAvailable()
    {
        return _inMemoryCache.Count > 0;
    }

    public DateTime? GetCacheLastUpdated()
    {
        return _cacheLastUpdated;
    }

    private void LoadCacheFromFile()
    {
        try
        {
            if (File.Exists(_cacheFilePath))
            {
                var json = File.ReadAllText(_cacheFilePath);
                var cacheData = JsonSerializer.Deserialize<CacheData>(json, _jsonOptions);
                
                if (cacheData != null && cacheData.Items != null)
                {
                    lock (_lock)
                    {
                        _inMemoryCache.Clear();
                        foreach (var item in cacheData.Items)
                        {
                            _inMemoryCache[item.Id] = item;
                        }
                        _cacheLastUpdated = cacheData.LastUpdated;
                    }
                }
            }
        }
        catch (Exception)
        {
            // If cache file is corrupted, start with empty cache
            _inMemoryCache.Clear();
        }
    }

    private async Task SaveCacheToFileAsync()
    {
        try
        {
            var cacheData = new CacheData
            {
                Items = _inMemoryCache.Values.ToList(),
                LastUpdated = _cacheLastUpdated ?? DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(cacheData, _jsonOptions);
            await File.WriteAllTextAsync(_cacheFilePath, json);
        }
        catch (Exception)
        {
            // If file write fails, cache remains in memory
        }
    }

    private class CacheData
    {
        public List<Item> Items { get; set; } = new();
        public DateTime? LastUpdated { get; set; }
    }
}

