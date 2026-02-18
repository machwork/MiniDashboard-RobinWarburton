using Microsoft.AspNetCore.Mvc;
using MiniDashboard.Api.DTOs;
using MiniDashboard.Api.Services;

namespace MiniDashboard.Api.Controllers;

/// <summary>
/// Controller for managing items
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all items
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems()
    {
        try
        {
            var items = await _itemService.GetAllItemsAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items");
            return StatusCode(500, "An error occurred while retrieving items");
        }
    }

    /// <summary>
    /// Get item by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItem(int id)
    {
        try
        {
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound($"Item with ID {id} not found");
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {ItemId}", id);
            return StatusCode(500, "An error occurred while retrieving the item");
        }
    }

    /// <summary>
    /// Search items by query string
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ItemDto>>> SearchItems([FromQuery] string query)
    {
        try
        {
            var items = await _itemService.SearchItemsAsync(query);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching items with query: {Query}", query);
            return StatusCode(500, "An error occurred while searching items");
        }
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemDto>> CreateItem([FromBody] CreateItemDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = await _itemService.CreateItemAsync(createDto);
            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item");
            return StatusCode(500, "An error occurred while creating the item");
        }
    }

    /// <summary>
    /// Create multiple items in bulk
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(IEnumerable<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ItemDto>>> CreateItemsBulk([FromBody] IEnumerable<CreateItemDto> createDtos)
    {
        try
        {
            if (createDtos == null || !createDtos.Any())
            {
                return BadRequest("At least one item is required");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var items = await _itemService.CreateItemsBulkAsync(createDtos);
            return StatusCode(201, items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating items in bulk");
            return StatusCode(500, "An error occurred while creating items");
        }
    }

    /// <summary>
    /// Update an existing item
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemDto>> UpdateItem(int id, [FromBody] UpdateItemDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var item = await _itemService.UpdateItemAsync(id, updateDto);
            if (item == null)
            {
                return NotFound($"Item with ID {id} not found");
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item {ItemId}", id);
            return StatusCode(500, "An error occurred while updating the item");
        }
    }

    /// <summary>
    /// Delete an item
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            var deleted = await _itemService.DeleteItemAsync(id);
            if (!deleted)
            {
                return NotFound($"Item with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {ItemId}", id);
            return StatusCode(500, "An error occurred while deleting the item");
        }
    }
}


