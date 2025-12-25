using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models.ViewModels;

public class CourseCreateViewModel
{
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
    public string? PreviewVideoUrl { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public bool IsFeatured { get; set; }
}

public class LessonCreateViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public string? VideoUrl { get; set; }
    public string? MaterialUrl { get; set; }
    public int OrderIndex { get; set; }
    public int Duration { get; set; } // Duration in minutes
    public int CourseId { get; set; }
}

public class QuizCreateViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public int TimeLimit { get; set; }
    public int PassingScore { get; set; }
    public int CourseId { get; set; }
    public List<QuestionCreateViewModel> Questions { get; set; } = new();
}

public class QuestionCreateViewModel
{
    [Required]
    [StringLength(500)]
    public string Text { get; set; } = string.Empty;
    
    public QuestionType Type { get; set; }
    public int Points { get; set; } = 1;
    public List<AnswerCreateViewModel> Answers { get; set; } = new();
}

public class AnswerCreateViewModel
{
    [Required]
    [StringLength(300)]
    public string Text { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; }
}

public class CourseDetailsViewModel
{
    public Course Course { get; set; } = null!;
    public bool IsEnrolled { get; set; }
    public bool CanEnroll { get; set; }
    public List<Lesson> Lessons { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public bool CanReview { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    public Lesson? CurrentLesson { get; set; }
    public List<Course> RelatedCourses { get; set; } = new();
}

public class StudentDashboardViewModel
{
    public List<Enrollment> EnrolledCourses { get; set; } = new();
    public List<Course> RecommendedCourses { get; set; } = new();
    public int TotalCourses { get; set; }
    public int CompletedCourses { get; set; }
    public List<Certificate> Certificates { get; set; } = new();
}

public class InstructorDashboardViewModel
{
    public List<Course> MyCourses { get; set; } = new();
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal TotalEarnings { get; set; }
    public List<Enrollment> RecentEnrollments { get; set; } = new();
    public QuizPerformanceViewModel QuizPerformance { get; set; } = new();
    public int CertificatesIssued { get; set; }
}

public class QuizPerformanceViewModel
{
    public int TotalAttempts { get; set; }
    public double AverageScore { get; set; }
    public double PassRate { get; set; }
}

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalCourses { get; set; }
    public int PendingApprovals { get; set; }
    public int TotalEnrollments { get; set; }
    public List<Course> PendingCourses { get; set; } = new();
    public List<ApplicationUser> RecentUsers { get; set; } = new();
}

public class ReviewCreateViewModel
{
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
    
    public int CourseId { get; set; }
}

public class UserManagementViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VideoPlayerViewModel
{
    public Lesson CurrentLesson { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public List<Lesson> Playlist { get; set; } = new();
    public bool IsEnrolled { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    public Lesson? NextLesson { get; set; }
    public Lesson? PreviousLesson { get; set; }
    public bool IsCompleted { get; set; }
}

public class CourseBrowseViewModel
{
    public List<Course> Courses { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? SearchTerm { get; set; }
    public int? SelectedCategoryId { get; set; }
    public string? PriceFilter { get; set; }
    public string? SortBy { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCourses { get; set; }
}