using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EasyLearn.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<LessonProgress> LessonProgresses { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<StudentAnswer> StudentAnswers { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<LoginEntry> LoginEntries { get; set; }
    public DbSet<RegistrationEntry> RegistrationEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany(u => u.CreatedCourses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizAttempt>()
            .HasOne(qa => qa.Student)
            .WithMany(u => u.QuizAttempts)
            .HasForeignKey(qa => qa.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StudentAnswer>()
            .HasOne(sa => sa.QuizAttempt)
            .WithMany(qa => qa.StudentAnswers)
            .HasForeignKey(sa => sa.QuizAttemptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAnswer>()
            .HasOne(sa => sa.Question)
            .WithMany()
            .HasForeignKey(sa => sa.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure decimal precision
        builder.Entity<Course>()
            .Property(c => c.Price)
            .HasPrecision(18, 2);

        // Configure Review relationships
        builder.Entity<Review>()
            .HasOne(r => r.Student)
            .WithMany()
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Review>()
            .HasOne(r => r.Course)
            .WithMany(c => c.Reviews)
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Certificate relationships
        builder.Entity<Certificate>()
            .HasOne(c => c.Student)
            .WithMany()
            .HasForeignKey(c => c.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Certificate>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Announcement relationships
        builder.Entity<Announcement>()
            .HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure UserProfile relationships
        builder.Entity<UserProfile>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure LoginEntry relationships
        builder.Entity<LoginEntry>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure RegistrationEntry relationships
        builder.Entity<RegistrationEntry>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Programming", Description = "Software development courses" },
            new Category { Id = 2, Name = "Design", Description = "UI/UX and graphic design courses" },
            new Category { Id = 3, Name = "Business", Description = "Business and management courses" },
            new Category { Id = 4, Name = "Marketing", Description = "Digital marketing courses" }
        );
    }
}