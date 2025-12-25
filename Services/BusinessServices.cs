using EasyLearn.Models;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace EasyLearn.Services;

public interface ICertificateService
{
    Task<Certificate> GenerateCertificateAsync(string studentId, int courseId);
    Task<byte[]> GeneratePdfCertificateAsync(Certificate certificate);
}

public class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _context;

    public CertificateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Certificate> GenerateCertificateAsync(string studentId, int courseId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (enrollment?.CompletedAt == null)
            throw new InvalidOperationException("Course not completed");

        var existingCertificate = await _context.Certificates
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);

        if (existingCertificate != null)
            return existingCertificate;

        var certificate = new Certificate
        {
            StudentId = studentId,
            CourseId = courseId,
            CertificateNumber = GenerateCertificateNumber(),
            IssuedAt = DateTime.UtcNow
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync();

        return certificate;
    }

    public async Task<byte[]> GeneratePdfCertificateAsync(Certificate certificate)
    {
        var cert = await _context.Certificates
            .Include(c => c.Student)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == certificate.Id);

        if (cert == null) throw new ArgumentException("Certificate not found");

        using var stream = new MemoryStream();
        var document = new Document(PageSize.A4.Rotate());
        PdfWriter.GetInstance(document, stream);
        
        document.Open();
        
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24);
        var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 14);
        
        document.Add(new Paragraph("Certificate of Completion", titleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"This is to certify that", bodyFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph($"{cert.Student.FirstName} {cert.Student.LastName}", titleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph($"has successfully completed the course", bodyFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph($"{cert.Course.Title}", titleFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Certificate Number: {cert.CertificateNumber}", bodyFont) { Alignment = Element.ALIGN_CENTER });
        document.Add(new Paragraph($"Date: {cert.IssuedAt:MMMM dd, yyyy}", bodyFont) { Alignment = Element.ALIGN_CENTER });
        
        document.Close();
        return stream.ToArray();
    }

    private string GenerateCertificateNumber()
    {
        return $"EL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

public interface IProgressService
{
    Task<int> CalculateCourseProgressAsync(string studentId, int courseId);
    Task UpdateLessonProgressAsync(string studentId, int lessonId, bool completed);
    Task<bool> IsCourseCompletedAsync(string studentId, int courseId);
}

public class ProgressService : IProgressService
{
    private readonly ApplicationDbContext _context;

    public ProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CalculateCourseProgressAsync(string studentId, int courseId)
    {
        var totalLessons = await _context.Lessons.CountAsync(l => l.CourseId == courseId && l.IsActive);
        if (totalLessons == 0) return 0;

        var completedLessons = await _context.LessonProgresses
            .CountAsync(lp => lp.StudentId == studentId && 
                             lp.Lesson.CourseId == courseId && 
                             lp.IsCompleted);

        return (int)Math.Round((double)completedLessons / totalLessons * 100);
    }

    public async Task UpdateLessonProgressAsync(string studentId, int lessonId, bool completed)
    {
        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                StudentId = studentId,
                LessonId = lessonId,
                IsCompleted = completed,
                CompletedAt = completed ? DateTime.UtcNow : null
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.IsCompleted = completed;
            progress.CompletedAt = completed ? DateTime.UtcNow : null;
        }

        await _context.SaveChangesAsync();

        // Update course enrollment progress
        var lesson = await _context.Lessons.FindAsync(lessonId);
        if (lesson != null)
        {
            var courseProgress = await CalculateCourseProgressAsync(studentId, lesson.CourseId);
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == lesson.CourseId);

            if (enrollment != null)
            {
                enrollment.Progress = courseProgress;
                if (courseProgress == 100 && enrollment.CompletedAt == null)
                {
                    enrollment.CompletedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<bool> IsCourseCompletedAsync(string studentId, int courseId)
    {
        var progress = await CalculateCourseProgressAsync(studentId, courseId);
        return progress == 100;
    }
}