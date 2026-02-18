using System.ComponentModel.DataAnnotations;

namespace MiniDashboard.Api.DTOs;

/// <summary>
/// Data Transfer Object for creating a new item
/// </summary>
public class CreateItemDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}



