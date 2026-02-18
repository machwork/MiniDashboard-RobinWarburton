using MiniDashboard.Api.DTOs;
using MiniDashboard.Api.Models;
using MiniDashboard.Api.Repositories;

namespace MiniDashboard.Api.Services;

/// <summary>
/// Service implementation for Item business logic
/// </summary>
public class ItemService : IItemService
{
    private readonly IItemRepository _repository;

    public ItemService(IItemRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<ItemDto>> GetAllItemsAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<ItemDto?> GetItemByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item == null ? null : MapToDto(item);
    }

    public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string query)
    {
        var items = await _repository.SearchAsync(query);
        return items.Select(MapToDto);
    }

    public async Task<ItemDto> CreateItemAsync(CreateItemDto createDto)
    {
        var item = new Item
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Category = createDto.Category,
            Price = createDto.Price
        };

        var createdItem = await _repository.CreateAsync(item);
        return MapToDto(createdItem);
    }

    public async Task<IEnumerable<ItemDto>> CreateItemsBulkAsync(IEnumerable<CreateItemDto> createDtos)
    {
        var createdItems = new List<ItemDto>();
        
        foreach (var createDto in createDtos)
        {
            var item = new Item
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Category = createDto.Category,
                Price = createDto.Price
            };
            
            var createdItem = await _repository.CreateAsync(item);
            createdItems.Add(MapToDto(createdItem));
        }
        
        return createdItems;
    }

    public async Task<ItemDto?> UpdateItemAsync(int id, UpdateItemDto updateDto)
    {
        var existingItem = await _repository.GetByIdAsync(id);
        if (existingItem == null)
        {
            return null;
        }

        var item = new Item
        {
            Id = id,
            Name = updateDto.Name,
            Description = updateDto.Description,
            Category = updateDto.Category,
            Price = updateDto.Price,
            CreatedDate = existingItem.CreatedDate
        };

        var updatedItem = await _repository.UpdateAsync(id, item);
        return updatedItem == null ? null : MapToDto(updatedItem);
    }

    public async Task<bool> DeleteItemAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static ItemDto MapToDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Category = item.Category,
            Price = item.Price,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate
        };
    }
}


