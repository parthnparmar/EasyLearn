using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class Announcement
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsPinned { get; set; } = false;
    
    // Foreign key
    public string CreatedById { get; set; } = string.Empty;
    
    // Navigation property
    public ApplicationUser CreatedBy { get; set; } = null!;
}