using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class Review
{
    public int Id { get; set; }
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = true;
    
    // Foreign keys
    public string StudentId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    
    // Navigation properties
    public ApplicationUser Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}