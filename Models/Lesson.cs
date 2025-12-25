using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class Lesson
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public string? VideoUrl { get; set; }
    public string? MaterialUrl { get; set; }
    public int OrderIndex { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int CourseId { get; set; }
    
    // Navigation properties
    public Course Course { get; set; } = null!;
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}