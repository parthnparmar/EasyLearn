using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class Course
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsApproved { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public string? PreviewVideoUrl { get; set; }
    
    // Foreign keys
    public string InstructorId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    
    // Navigation properties
    public ApplicationUser Instructor { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    // Computed properties
    public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
    public int TotalEnrollments => Enrollments.Count;
    public bool IsFree => Price == 0;
}