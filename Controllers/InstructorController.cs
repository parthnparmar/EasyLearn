using EasyLearn.Models;
using EasyLearn.Models.ViewModels;
using EasyLearn.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyLearn.Controllers;

[Authorize(Roles = "Instructor")]
[Route("instructor")]
public class InstructorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IProfileService _profileService;

    public InstructorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProfileService profileService)
    {
        _context = context;
        _userManager = userManager;
        _profileService = profileService;
    }

    [Route("dashboard")]
    public async Task<IActionResult> Index()
    {
        return await Dashboard();
    }

    [Route("")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get or create profile
            var profile = await _profileService.GetOrCreateProfileAsync(instructorId);
            ViewBag.Profile = profile;

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();

            var recentEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.Course.InstructorId == instructorId)
                .OrderByDescending(e => e.EnrolledAt)
                .Take(5)
                .ToListAsync();

            // Get quiz performance data
            var quizAttempts = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                .ThenInclude(q => q.Course)
                .Where(qa => qa.Quiz.Course.InstructorId == instructorId)
                .ToListAsync();

            // Get certificates issued
            var certificates = await _context.Certificates
                .Include(c => c.Course)
                .Where(c => c.Course.InstructorId == instructorId)
                .CountAsync();

            var viewModel = new InstructorDashboardViewModel
            {
                TotalCourses = courses?.Count ?? 0,
                TotalStudents = courses?.SelectMany(c => c.Enrollments ?? new List<Enrollment>())
                    .Select(e => e.StudentId).Distinct().Count() ?? 0,
                TotalEarnings = courses?.Where(c => c.Price > 0)
                    .Sum(c => (c.Enrollments?.Count ?? 0) * c.Price) ?? 0,
                MyCourses = courses?.OrderByDescending(c => c.CreatedAt).Take(5).ToList() ?? new List<Course>(),
                RecentEnrollments = recentEnrollments ?? new List<Enrollment>(),
                QuizPerformance = new QuizPerformanceViewModel
                {
                    TotalAttempts = quizAttempts?.Count ?? 0,
                    AverageScore = quizAttempts?.Any() == true ? quizAttempts.Average(qa => qa.Percentage) : 0,
                    PassRate = quizAttempts?.Any() == true ? 
                        (double)quizAttempts.Count(qa => qa.IsPassed) / quizAttempts.Count * 100 : 0
                },
                CertificatesIssued = certificates
            };

            return View(viewModel);
        }
        catch (Exception)
        {
            // Log the exception
            TempData["Error"] = "An error occurred while loading the dashboard.";
            return View(new InstructorDashboardViewModel());
        }
    }

    [Route("my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        var instructorId = _userManager.GetUserId(User)!;
        var courses = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews)
            .Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(courses);
    }

    [HttpGet]
    [Route("create-course")]
    public async Task<IActionResult> CreateCourse()
    {
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        return View();
    }

    [HttpPost]
    [Route("create-course")]
    public async Task<IActionResult> CreateCourse(CourseCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                ThumbnailUrl = model.ThumbnailUrl,
                CategoryId = model.CategoryId,
                InstructorId = _userManager.GetUserId(User)!,
                IsFeatured = model.IsFeatured
            };
            
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyCourses));
        }
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        return View(model);
    }

    [Route("manage-content/{courseId:int}")]
    public async Task<IActionResult> ManageContent(int courseId)
    {
        try
        {
            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            
            if (course == null) 
            {
                TempData["Error"] = "Course not found or access denied";
                return RedirectToAction("MyCourses");
            }
            
            return View(course);
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while loading course content";
            return RedirectToAction("MyCourses");
        }
    }

    [Route("manage-lessons/{courseId:int}")]
    public async Task<IActionResult> ManageLessons(int courseId)
    {
        try
        {
            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            
            if (course == null) 
            {
                TempData["Error"] = "Course not found or access denied";
                return RedirectToAction("MyCourses");
            }
            
            return View(course);
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while loading lessons";
            return RedirectToAction("MyCourses");
        }
    }

    [HttpPost]
    [Route("create-lesson")]
    public async Task<IActionResult> CreateLesson(LessonCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid lesson data" });
            }

            // Verify course belongs to instructor
            var instructorId = _userManager.GetUserId(User);
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == model.CourseId && c.InstructorId == instructorId);
            
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found or access denied" });
            }

            var lesson = new Lesson
            {
                Title = model.Title ?? string.Empty,
                Description = model.Description ?? string.Empty,
                VideoUrl = model.VideoUrl,
                MaterialUrl = model.MaterialUrl,
                OrderIndex = model.OrderIndex,
                Duration = TimeSpan.FromMinutes(model.Duration),
                CourseId = model.CourseId
            };
            
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Lesson created successfully!";
            return Json(new { success = true, lessonId = lesson.Id });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "An error occurred while creating the lesson" });
        }
    }

    [HttpPost]
    [Route("upload-material")]
    public async Task<IActionResult> UploadMaterial(IFormFile file, int courseId)
    {
        if (file != null && file.Length > 0)
        {
            var allowedTypes = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            
            if (allowedTypes.Contains(extension))
            {
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine("wwwroot/uploads/materials", fileName);
                
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                return Json(new { success = true, url = $"/uploads/materials/{fileName}" });
            }
        }
        return Json(new { success = false, message = "Invalid file" });
    }

    [Route("manage-quizzes/{courseId:int}")]
    public async Task<IActionResult> ManageQuizzes(int courseId)
    {
        try
        {
            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Quizzes)
                .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            
            if (course == null) 
            {
                TempData["Error"] = "Course not found or access denied";
                return RedirectToAction("MyCourses");
            }
            
            return View(course);
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while loading quizzes";
            return RedirectToAction("MyCourses");
        }
    }

    [HttpGet]
    [Route("create-quiz/{courseId:int}")]
    public async Task<IActionResult> CreateQuiz(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course?.InstructorId != _userManager.GetUserId(User)) return NotFound();
        
        ViewBag.CourseId = courseId;
        return View();
    }

    [HttpPost]
    [Route("create-quiz")]
    public async Task<IActionResult> CreateQuiz(QuizCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid quiz data" });
            }

            // Verify course belongs to instructor
            var instructorId = _userManager.GetUserId(User);
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == model.CourseId && c.InstructorId == instructorId);
            
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found or access denied" });
            }

            var quiz = new Quiz
            {
                Title = model.Title ?? string.Empty,
                Description = model.Description ?? string.Empty,
                TimeLimit = model.TimeLimit,
                PassingScore = model.PassingScore,
                CourseId = model.CourseId
            };
            
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            
            if (model.Questions?.Any() == true)
            {
                foreach (var questionModel in model.Questions)
                {
                    var question = new Question
                    {
                        Text = questionModel.Text ?? string.Empty,
                        Type = questionModel.Type,
                        Points = questionModel.Points,
                        QuizId = quiz.Id
                    };
                    
                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();
                    
                    if (questionModel.Answers?.Any() == true)
                    {
                        foreach (var answerModel in questionModel.Answers)
                        {
                            var answer = new Answer
                            {
                                Text = answerModel.Text ?? string.Empty,
                                IsCorrect = answerModel.IsCorrect,
                                QuestionId = question.Id
                            };
                            _context.Answers.Add(answer);
                        }
                    }
                }
            }
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Quiz created successfully!";
            return Json(new { success = true, quizId = quiz.Id });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "An error occurred while creating the quiz" });
        }
    }

    [Route("manage-questions/{quizId:int}")]
    public async Task<IActionResult> ManageQuestions(int quizId)
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.Id == quizId && q.Course.InstructorId == _userManager.GetUserId(User));
        
        if (quiz == null) return NotFound();
        return View(quiz);
    }

    [HttpPost]
    [Route("create-question")]
    public async Task<IActionResult> CreateQuestion(Question question, List<Answer> answers)
    {
        try
        {
            if (!ModelState.IsValid || question == null)
            {
                TempData["Error"] = "Invalid question data";
                return RedirectToAction(nameof(ManageQuestions), new { quizId = question?.QuizId ?? 0 });
            }

            // Verify quiz belongs to instructor
            var instructorId = _userManager.GetUserId(User);
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == question.QuizId && q.Course.InstructorId == instructorId);
            
            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or access denied";
                return RedirectToAction("Dashboard");
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            if (answers?.Any() == true)
            {
                foreach (var answer in answers.Where(a => !string.IsNullOrEmpty(a.Text)))
                {
                    answer.QuestionId = question.Id;
                    _context.Answers.Add(answer);
                }
                await _context.SaveChangesAsync();
            }
            
            TempData["Success"] = "Question added successfully!";
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while creating the question";
        }
        
        return RedirectToAction(nameof(ManageQuestions), new { quizId = question.QuizId });
    }

    [Route("issue-certificate/{courseId:int}/{studentId}")]
    public async Task<IActionResult> IssueCertificate(int courseId, string studentId)
    {
        try
        {
            var instructorId = _userManager.GetUserId(User);
            
            // Verify course belongs to instructor
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            
            if (course == null)
            {
                return NotFound();
            }

            // Check if student has completed all requirements
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);
            
            if (enrollment == null || !enrollment.IsCompleted)
            {
                TempData["Error"] = "Student has not completed the course requirements";
                return RedirectToAction("StudentPerformance", new { courseId });
            }

            // Check if certificate already exists
            var existingCertificate = await _context.Certificates
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.StudentId == studentId);
            
            if (existingCertificate != null)
            {
                TempData["Info"] = "Certificate already issued for this student";
                return RedirectToAction("StudentPerformance", new { courseId });
            }

            // Create certificate
            var certificate = new Certificate
            {
                StudentId = studentId,
                CourseId = courseId,
                CertificateNumber = GenerateCertificateNumber(),
                IssuedAt = DateTime.UtcNow,
                FilePath = "" // Will be generated by certificate service
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Certificate issued successfully!";
            return RedirectToAction("StudentPerformance", new { courseId });
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while issuing the certificate";
            return RedirectToAction("StudentPerformance", new { courseId });
        }
    }

    [Route("export-performance/{courseId:int}")]
    public async Task<IActionResult> ExportPerformance(int courseId)
    {
        try
        {
            var instructorId = _userManager.GetUserId(User);
            
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId && e.Course.InstructorId == instructorId)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Student Name,Email,Enrolled Date,Progress %,Status,Completed Date");

            foreach (var enrollment in enrollments)
            {
                csv.AppendLine($"{enrollment.Student?.FirstName} {enrollment.Student?.LastName},{enrollment.Student?.Email},{enrollment.EnrolledAt:yyyy-MM-dd},{enrollment.ProgressPercentage:F1},{(enrollment.IsCompleted ? "Completed" : "In Progress")},{enrollment.CompletedAt?.ToString("yyyy-MM-dd") ?? ""}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"student-performance-{courseId}-{DateTime.Now:yyyyMMdd}.csv");
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while exporting data";
            return RedirectToAction("StudentPerformance", new { courseId });
        }
    }

    private string GenerateCertificateNumber()
    {
        return $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    [HttpGet]
    [Route("edit-course/{id:int}")]
    public async Task<IActionResult> EditCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == _userManager.GetUserId(User));
        
        if (course == null) return NotFound();
        
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        
        var model = new CourseCreateViewModel
        {
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            ImageUrl = course.ImageUrl,
            CategoryId = course.CategoryId
        };
        
        ViewBag.CourseId = id;
        return View(model);
    }

    [HttpPost]
    [Route("edit-course/{id:int}")]
    public async Task<IActionResult> EditCourse(int id, CourseCreateViewModel model)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == _userManager.GetUserId(User));
        if (course == null) return NotFound();
        
        if (ModelState.IsValid)
        {
            course.Title = model.Title;
            course.Description = model.Description;
            course.Price = model.Price;
            course.ImageUrl = model.ImageUrl;
            course.CategoryId = model.CategoryId;
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course updated successfully.";
            return RedirectToAction(nameof(MyCourses));
        }
        
        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        ViewBag.CourseId = id;
        return View(model);
    }

    [Route("student-performance/{courseId:int}")]
    public async Task<IActionResult> StudentPerformance(int courseId)
    {
        try
        {
            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Verify course belongs to instructor
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            
            if (course == null)
            {
                return NotFound();
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
            
            var quizResults = await _context.QuizAttempts
                .Include(qa => qa.Student)
                .Include(qa => qa.Quiz)
                .Where(qa => qa.Quiz.CourseId == courseId)
                .GroupBy(qa => qa.StudentId)
                .Select(g => new {
                    StudentId = g.Key,
                    AverageScore = g.Average(qa => qa.TotalPoints > 0 ? (double)qa.Score / qa.TotalPoints * 100 : 0),
                    AttemptCount = g.Count(),
                    BestScore = g.Max(qa => qa.TotalPoints > 0 ? (double)qa.Score / qa.TotalPoints * 100 : 0)
                })
                .ToListAsync();
            
            // Get lesson progress
            var lessonProgress = await _context.LessonProgresses
                .Include(lp => lp.Lesson)
                .Where(lp => lp.Lesson.CourseId == courseId)
                .GroupBy(lp => lp.StudentId)
                .Select(g => new {
                    StudentId = g.Key,
                    CompletedLessons = g.Count(lp => lp.IsCompleted),
                    TotalLessons = _context.Lessons.Count(l => l.CourseId == courseId)
                })
                .ToListAsync();
            
            ViewBag.QuizResults = quizResults.Cast<object>().ToList();
            ViewBag.LessonProgress = lessonProgress.Cast<object>().ToList();
            ViewBag.Course = course;
            
            return View(enrollments ?? new List<Enrollment>());
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while loading student performance data.";
            return RedirectToAction("Dashboard");
        }
    }
}