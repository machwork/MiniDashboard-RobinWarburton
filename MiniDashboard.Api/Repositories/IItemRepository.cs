using MiniDashboard.Api.Models;

namespace MiniDashboard.Api.Repositories;

/// <summary>
/// Repository interface for Item data access
/// </summary>
public interface IItemRepository
{
    Task<IEnumerable<Item>> GetAllAsync();
    Task<Item?> GetByIdAsync(int id);
    Task<IEnumerable<Item>> SearchAsync(string query);
    Task<Item> CreateAsync(Item item);
    Task<Item?> UpdateAsync(int id, Item item);
    Task<bool> DeleteAsync(int id);
}



