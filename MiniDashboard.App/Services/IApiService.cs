using MiniDashboard.App.Models;

namespace MiniDashboard.App.Services;

/// <summary>
/// Interface for API service to communicate with the backend
/// </summary>
public interface IApiService
{
    Task<IEnumerable<Item>> GetItemsAsync();
    Task<Item?> GetItemByIdAsync(int id);
    Task<IEnumerable<Item>> SearchItemsAsync(string query);
    Task<Item> CreateItemAsync(Item item);
    Task<Item?> UpdateItemAsync(int id, Item item);
    Task<bool> DeleteItemAsync(int id);
}



