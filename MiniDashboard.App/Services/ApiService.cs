using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MiniDashboard.App.Models;

namespace MiniDashboard.App.Services;

/// <summary>
/// Service implementation for API communication
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<ApiService> _logger;
    private readonly ICacheService? _cacheService;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger, ICacheService? cacheService = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/items");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<IEnumerable<Item>>(_jsonOptions);
            var itemsList = items?.ToList() ?? new List<Item>();
            
            // Update cache on successful API call
            if (_cacheService != null && itemsList.Any())
            {
                await _cacheService.SaveItemsToCacheAsync(itemsList);
            }
            
            return itemsList;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve items from API, attempting to use cache");
            
            // Fallback to cache if available
            if (_cacheService != null && _cacheService.IsCacheAvailable())
            {
                _logger.LogInformation("Using cached items as fallback");
                return await _cacheService.GetCachedItemsAsync();
            }
            
            throw new Exception($"Failed to retrieve items: {ex.Message}", ex);
        }
    }

    public async Task<Item?> GetItemByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/items/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            var item = await response.Content.ReadFromJsonAsync<Item>(_jsonOptions);
            
            // Update cache on successful API call
            if (_cacheService != null && item != null)
            {
                await _cacheService.SaveItemToCacheAsync(item);
            }
            
            return item;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve item {ItemId} from API, attempting to use cache", id);
            
            // Fallback to cache if available
            if (_cacheService != null && _cacheService.IsCacheAvailable())
            {
                _logger.LogInformation("Using cached item as fallback");
                return await _cacheService.GetCachedItemByIdAsync(id);
            }
            
            throw new Exception($"Failed to retrieve item: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Item>> SearchItemsAsync(string query)
    {
        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var response = await _httpClient.GetAsync($"api/items/search?query={encodedQuery}");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<IEnumerable<Item>>(_jsonOptions);
            return items ?? Enumerable.Empty<Item>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to search items with query '{Query}', attempting to use cache", query);
            
            // Fallback to cache if available
            if (_cacheService != null && _cacheService.IsCacheAvailable())
            {
                _logger.LogInformation("Using cached search results as fallback");
                return await _cacheService.SearchCachedItemsAsync(query);
            }
            
            throw new Exception($"Failed to search items: {ex.Message}", ex);
        }
    }

    public async Task<Item> CreateItemAsync(Item item)
    {
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                name = item.Name,
                description = item.Description,
                category = item.Category,
                price = item.Price
            }, _jsonOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/items", content);
            response.EnsureSuccessStatusCode();
            
            var createdItem = await response.Content.ReadFromJsonAsync<Item>(_jsonOptions);
            var result = createdItem ?? throw new Exception("Failed to deserialize created item");
            
            // Update cache on successful creation
            if (_cacheService != null)
            {
                await _cacheService.SaveItemToCacheAsync(result);
            }
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to create item via API");
            throw new Exception($"Failed to create item: {ex.Message}", ex);
        }
    }

    public async Task<Item?> UpdateItemAsync(int id, Item item)
    {
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                name = item.Name,
                description = item.Description,
                category = item.Category,
                price = item.Price
            }, _jsonOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/items/{id}", content);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            
            response.EnsureSuccessStatusCode();
            var updatedItem = await response.Content.ReadFromJsonAsync<Item>(_jsonOptions);
            
            // Update cache on successful update
            if (_cacheService != null && updatedItem != null)
            {
                await _cacheService.SaveItemToCacheAsync(updatedItem);
            }
            
            return updatedItem;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to update item {ItemId} via API", id);
            throw new Exception($"Failed to update item: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteItemAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/items/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            response.EnsureSuccessStatusCode();
            
            // Update cache on successful deletion
            if (_cacheService != null)
            {
                await _cacheService.RemoveItemFromCacheAsync(id);
            }
            
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to delete item {ItemId} via API", id);
            throw new Exception($"Failed to delete item: {ex.Message}", ex);
        }
    }
}

