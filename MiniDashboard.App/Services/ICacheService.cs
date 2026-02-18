using MiniDashboard.App.Models;

namespace MiniDashboard.App.Services;

/// <summary>
/// Service for caching items locally for offline mode
/// </summary>
public interface ICacheService
{
    Task<IEnumerable<Item>> GetCachedItemsAsync();
    Task<Item?> GetCachedItemByIdAsync(int id);
    Task<IEnumerable<Item>> SearchCachedItemsAsync(string query);
    Task SaveItemsToCacheAsync(IEnumerable<Item> items);
    Task SaveItemToCacheAsync(Item item);
    Task RemoveItemFromCacheAsync(int id);
    Task ClearCacheAsync();
    bool IsCacheAvailable();
    DateTime? GetCacheLastUpdated();
}

