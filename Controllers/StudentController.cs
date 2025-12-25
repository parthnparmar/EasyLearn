using EasyLearn.Models;
using EasyLearn.Models.ViewModels;
using EasyLearn.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyLearn.Controllers;

[Authorize(Roles = "Student")]
[Route("student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProgressService _progressService;
    private readonly ICertificateService _certificateService;
    private readonly IProfileService _profileService;

    public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
        IProgressService progressService, ICertificateService certificateService, IProfileService profileService)
    {
        _context = context;
        _userManager = userManager;
        _progressService = progressService;
        _certificateService = certificateService;
        _profileService = profileService;
    }

    [Route("dashboard")]
    public async Task<IActionResult> Index()
    {
        var studentId = _userManager.GetUserId(User)!;
        
        // Get or create profile
        var profile = await _profileService.GetOrCreateProfileAsync(studentId);
        ViewBag.Profile = profile;
        
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();

        var certificates = await _context.Certificates
            .Include(c => c.Course)
            .Where(c => c.StudentId == studentId)
            .ToListAsync();

        var recommendedCourses = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Reviews)
            .Where(c => c.IsApproved && c.IsActive)
            .OrderByDescending(c => c.Reviews.Average(r => (double?)r.Rating) ?? 0)
            .Take(6)
            .ToListAsync();

        var viewModel = new StudentDashboardViewModel
        {
            EnrolledCourses = enrollments,
            TotalCourses = enrollments.Count,
            CompletedCourses = enrollments.Count(e => e.CompletedAt != null),
            Certificates = certificates,
            RecommendedCourses = recommendedCourses
        };

        return View(viewModel);
    }

    [HttpGet]
    [Route("courses")]
    public async Task<IActionResult> BrowseCourses(int? categoryId, string? search, string? priceFilter, string? sortBy, int page = 1)
    {
        const int pageSize = 12;
        
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Reviews)
            .Where(c => c.IsApproved && c.IsActive);

        if (categoryId.HasValue)
            query = query.Where(c => c.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));

        if (!string.IsNullOrEmpty(priceFilter))
        {
            switch (priceFilter)
            {
                case "free":
                    query = query.Where(c => c.Price == 0);
                    break;
                case "paid":
                    query = query.Where(c => c.Price > 0);
                    break;
            }
        }

        query = sortBy switch
        {
            "price_low" => query.OrderBy(c => c.Price),
            "price_high" => query.OrderByDescending(c => c.Price),
            "rating" => query.OrderByDescending(c => c.Reviews.Average(r => (double?)r.Rating) ?? 0),
            "newest" => query.OrderByDescending(c => c.CreatedAt),
            "popular" => query.OrderByDescending(c => c.TotalEnrollments),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCourses = await query.CountAsync();
        var courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new CourseBrowseViewModel
        {
            Courses = courses,
            Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
            SearchTerm = search,
            SelectedCategoryId = categoryId,
            PriceFilter = priceFilter,
            SortBy = sortBy,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCourses / (double)pageSize),
            TotalCourses = totalCourses
        };

        return View("BrowseCoursesModern", viewModel);
    }

    [HttpGet]
    [Route("course/{id:int}")]
    public async Task<IActionResult> CourseDetails(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
            .Include(c => c.Quizzes)
            .Include(c => c.Reviews)
            .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsApproved);

        if (course == null) return NotFound();

        var studentId = _userManager.GetUserId(User)!;
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == studentId);

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.CourseId == id && r.StudentId == studentId);

        // Calculate progress if enrolled
        var completedLessons = 0;
        var progressPercentage = 0.0;
        Lesson? currentLesson = null;
        
        if (enrollment != null)
        {
            completedLessons = await _context.LessonProgresses
                .CountAsync(lp => lp.StudentId == studentId && 
                                 course.Lessons.Select(l => l.Id).Contains(lp.LessonId) && 
                                 lp.IsCompleted);
            
            if (course.Lessons.Any())
            {
                progressPercentage = (completedLessons * 100.0) / course.Lessons.Count;
                
                // Find current lesson (first incomplete or first lesson)
                var incompleteLessons = course.Lessons
                    .Where(l => !_context.LessonProgresses
                        .Any(lp => lp.LessonId == l.Id && lp.StudentId == studentId && lp.IsCompleted))
                    .OrderBy(l => l.OrderIndex)
                    .ToList();
                    
                currentLesson = incompleteLessons.FirstOrDefault() ?? course.Lessons.OrderBy(l => l.OrderIndex).FirstOrDefault();
            }
        }

        // Get related courses
        var relatedCourses = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Reviews)
            .Where(c => c.CategoryId == course.CategoryId && c.Id != id && c.IsApproved && c.IsActive)
            .OrderByDescending(c => c.Reviews.Average(r => (double?)r.Rating) ?? 0)
            .Take(4)
            .ToListAsync();

        // Increment view count
        course.ViewCount++;
        await _context.SaveChangesAsync();

        var viewModel = new CourseDetailsViewModel
        {
            Course = course,
            IsEnrolled = enrollment != null,
            CanEnroll = enrollment == null && course.IsActive,
            Lessons = course.Lessons.ToList(),
            Reviews = course.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
            CanReview = enrollment?.CompletedAt != null && existingReview == null,
            CompletedLessons = completedLessons,
            ProgressPercentage = progressPercentage,
            CurrentLesson = currentLesson,
            RelatedCourses = relatedCourses
        };

        return View("CourseDetailsModern", viewModel);
    }

    [HttpPost]
    [Route("enroll-course")]
    public async Task<IActionResult> EnrollCourse(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        var studentId = _userManager.GetUserId(User)!;
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

        if (existingEnrollment != null)
        {
            TempData["Info"] = "You are already enrolled in this course.";
            return RedirectToAction(nameof(CourseDetails), new { id = courseId });
        }

        // For paid courses, redirect to payment (placeholder for now)
        if (course.Price > 0)
        {
            TempData["Info"] = "Payment integration coming soon! For now, paid courses are treated as free.";
        }

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = studentId,
            EnrolledAt = DateTime.UtcNow
        };
        
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Successfully enrolled in the course!";
        return RedirectToAction(nameof(CourseDetails), new { id = courseId });
    }

    [Route("my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        var studentId = _userManager.GetUserId(User);
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Category)
            .Include(e => e.Course)
            .ThenInclude(c => c.Instructor)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();

        return View(enrollments);
    }

    [HttpGet]
    [Route("watch/{lessonId:int}")]
    public async Task<IActionResult> WatchLesson(int lessonId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .ThenInclude(c => c.Instructor)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null) return NotFound();

        var studentId = _userManager.GetUserId(User)!;
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == lesson.CourseId && e.StudentId == studentId);

        if (enrollment == null) return Forbid();

        var allLessons = await _context.Lessons
            .Where(l => l.CourseId == lesson.CourseId && l.IsActive)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();

        var currentIndex = allLessons.FindIndex(l => l.Id == lessonId);
        var nextLesson = currentIndex < allLessons.Count - 1 ? allLessons[currentIndex + 1] : null;
        var previousLesson = currentIndex > 0 ? allLessons[currentIndex - 1] : null;

        var completedLessons = await _context.LessonProgresses
            .CountAsync(lp => lp.StudentId == studentId && 
                             allLessons.Select(l => l.Id).Contains(lp.LessonId) && 
                             lp.IsCompleted);

        var progressPercentage = allLessons.Count > 0 ? (completedLessons * 100.0) / allLessons.Count : 0;

        var isCompleted = await _context.LessonProgresses
            .AnyAsync(lp => lp.LessonId == lessonId && lp.StudentId == studentId && lp.IsCompleted);

        var viewModel = new VideoPlayerViewModel
        {
            CurrentLesson = lesson,
            Course = lesson.Course,
            Playlist = allLessons,
            IsEnrolled = true,
            CompletedLessons = completedLessons,
            ProgressPercentage = progressPercentage,
            NextLesson = nextLesson,
            PreviousLesson = previousLesson,
            IsCompleted = isCompleted
        };
        
        return View("VideoPlayer", viewModel);
    }

    [HttpPost]
    [Route("complete-lesson/{lessonId:int}")]
    public async Task<IActionResult> CompleteLesson(int lessonId)
    {
        var studentId = _userManager.GetUserId(User)!;
        await _progressService.UpdateLessonProgressAsync(studentId, lessonId, true);
        return Ok();
    }

    [HttpGet]
    [Route("quiz/{quizId:int}")]
    public async Task<IActionResult> TakeQuiz(int quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null) return NotFound();

        var studentId = _userManager.GetUserId(User)!;
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == quiz.CourseId && e.StudentId == studentId);

        if (enrollment == null) return Forbid();

        return View(quiz);
    }

    [HttpPost]
    [Route("submit-quiz/{quizId:int}")]
    public async Task<IActionResult> SubmitQuiz(int quizId, Dictionary<int, string> answers)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null) return NotFound();

        var studentId = _userManager.GetUserId(User)!;
        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            StudentId = studentId,
            AttemptedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        int score = 0;
        int totalPoints = quiz.Questions.Sum(q => q.Points);

        foreach (var question in quiz.Questions)
        {
            if (answers.TryGetValue(question.Id, out var answerText))
            {
                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                var isCorrect = question.Type == QuestionType.TrueFalse 
                    ? correctAnswer?.Text.Equals(answerText, StringComparison.OrdinalIgnoreCase) == true
                    : question.Answers.Any(a => a.IsCorrect && a.Text == answerText);
                
                if (isCorrect) score += question.Points;

                var studentAnswer = new StudentAnswer
                {
                    QuestionId = question.Id,
                    AnswerText = answerText,
                    IsCorrect = isCorrect,
                    Points = isCorrect ? question.Points : 0
                };
                attempt.StudentAnswers.Add(studentAnswer);
            }
        }

        attempt.Score = score;
        attempt.TotalPoints = totalPoints;
        attempt.Percentage = totalPoints > 0 ? (double)score / totalPoints * 100 : 0;
        attempt.IsPassed = attempt.Percentage >= quiz.PassingScore;

        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        TempData["QuizResult"] = attempt.IsPassed ? "Congratulations! You passed the quiz." : "You didn't pass this time. Keep studying and try again!";
        return RedirectToAction(nameof(QuizResult), new { attemptId = attempt.Id });
    }

    [HttpPost]
    [Route("add-review")]
    public async Task<IActionResult> AddReview(ReviewCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });

        var studentId = _userManager.GetUserId(User)!;
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == model.CourseId && e.StudentId == studentId && e.CompletedAt != null);

        if (enrollment == null)
            return Forbid();

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.CourseId == model.CourseId && r.StudentId == studentId);

        if (existingReview != null)
            return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });

        var review = new Review
        {
            CourseId = model.CourseId,
            StudentId = studentId,
            Rating = model.Rating,
            Comment = model.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
    }

    [HttpGet]
    [Route("certificate/{courseId:int}")]
    public async Task<IActionResult> DownloadCertificate(int courseId)
    {
        var studentId = _userManager.GetUserId(User)!;
        
        try
        {
            var certificate = await _certificateService.GenerateCertificateAsync(studentId, courseId);
            var pdfBytes = await _certificateService.GeneratePdfCertificateAsync(certificate);
            
            return File(pdfBytes, "application/pdf", $"Certificate-{certificate.CertificateNumber}.pdf");
        }
        catch (InvalidOperationException)
        {
            TempData["Error"] = "Course not completed yet.";
            return RedirectToAction(nameof(CourseDetails), new { id = courseId });
        }
    }

    [HttpGet]
    [Route("download-material/{lessonId:int}")]
    public async Task<IActionResult> DownloadMaterial(int lessonId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null || string.IsNullOrEmpty(lesson.MaterialUrl))
            return NotFound();

        var studentId = _userManager.GetUserId(User)!;
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == lesson.CourseId && e.StudentId == studentId);

        if (enrollment == null) return Forbid();

        // For demo purposes, we'll redirect to the material URL
        // In production, you'd want to serve files from secure storage
        return Redirect(lesson.MaterialUrl);
    }

    [HttpPost]
    [Route("update-progress")]
    public async Task<IActionResult> UpdateProgress(int lessonId, int watchTime)
    {
        var studentId = _userManager.GetUserId(User)!;
        
        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                StudentId = studentId,
                LessonId = lessonId,
                StartedAt = DateTime.UtcNow,
                WatchTime = watchTime
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.WatchTime = Math.Max(progress.WatchTime, watchTime);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("quiz-result/{attemptId:int}")]
    public async Task<IActionResult> QuizResult(int attemptId)
    {
        var attempt = await _context.QuizAttempts
            .Include(qa => qa.Quiz)
            .Include(qa => qa.StudentAnswers)
            .ThenInclude(sa => sa.Question)
            .FirstOrDefaultAsync(qa => qa.Id == attemptId && qa.StudentId == _userManager.GetUserId(User));

        if (attempt == null) return NotFound();
        return View(attempt);
    }
}
