using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class Enrollment
{
    public int Id { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; } = false;
    public double ProgressPercentage { get; set; } = 0;
    public int Progress { get; set; } = 0;
    
    // Foreign keys
    public string StudentId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    
    // Navigation properties
    public ApplicationUser Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

public class LessonProgress
{
    public int Id { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public int WatchTime { get; set; } = 0; // in seconds
    
    // Foreign keys
    public string StudentId { get; set; } = string.Empty;
    public int LessonId { get; set; }
    
    // Navigation properties
    public ApplicationUser Student { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}

public class QuizAttempt
{
    public int Id { get; set; }
    public int Score { get; set; }
    public double Percentage { get; set; }
    public int TotalPoints { get; set; }
    public bool IsPassed { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    
    // Foreign keys
    public string StudentId { get; set; } = string.Empty;
    public int QuizId { get; set; }
    
    // Navigation properties
    public ApplicationUser Student { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
    public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
}

public class StudentAnswer
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Points { get; set; }
    
    // Foreign keys
    public int QuizAttemptId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedAnswerId { get; set; }
    
    // Navigation properties
    public QuizAttempt QuizAttempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public Answer? SelectedAnswer { get; set; }
}

public class Certificate
{
    public int Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string FilePath { get; set; } = string.Empty;
    
    // Foreign keys
    public string StudentId { get; set; } = string.Empty;
    public int CourseId { get; set; }
    
    // Navigation properties
    public ApplicationUser Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}