using MiniDashboard.Api.DTOs;
using MiniDashboard.Api.Models;

namespace MiniDashboard.Api.Services;

/// <summary>
/// Service interface for Item business logic
/// </summary>
public interface IItemService
{
    Task<IEnumerable<ItemDto>> GetAllItemsAsync();
    Task<ItemDto?> GetItemByIdAsync(int id);
    Task<IEnumerable<ItemDto>> SearchItemsAsync(string query);
    Task<ItemDto> CreateItemAsync(CreateItemDto createDto);
    Task<IEnumerable<ItemDto>> CreateItemsBulkAsync(IEnumerable<CreateItemDto> createDtos);
    Task<ItemDto?> UpdateItemAsync(int id, UpdateItemDto updateDto);
    Task<bool> DeleteItemAsync(int id);
}


