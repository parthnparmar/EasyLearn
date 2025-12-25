using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class Quiz
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public int TimeLimit { get; set; } // in minutes
    public int PassingScore { get; set; } // percentage
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int CourseId { get; set; }
    
    // Navigation properties
    public Course Course { get; set; } = null!;
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}

public class Question
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Text { get; set; } = string.Empty;
    
    public QuestionType Type { get; set; }
    public int Points { get; set; } = 1;
    public int OrderIndex { get; set; }
    
    // Foreign key
    public int QuizId { get; set; }
    
    // Navigation properties
    public Quiz Quiz { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

public class Answer
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(300)]
    public string Text { get; set; } = string.Empty;
    
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
    
    // Foreign key
    public int QuestionId { get; set; }
    
    // Navigation property
    public Question Question { get; set; } = null!;
}

public enum QuestionType
{
    MultipleChoice = 1,
    TrueFalse = 2,
    ShortAnswer = 3
}