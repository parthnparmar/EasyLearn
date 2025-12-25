using EasyLearn.Models;
using EasyLearn.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EasyLearn.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Route("dashboard")]
    public async Task<IActionResult> Index()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalCourses = await _context.Courses.CountAsync();
        var pendingApprovals = await _context.Courses.CountAsync(c => !c.IsApproved);
        var totalEnrollments = await _context.Enrollments.CountAsync();
        
        var pendingCourses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Where(c => !c.IsApproved)
            .Take(5)
            .ToListAsync();
            
        var recentUsers = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(10)
            .ToListAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalCourses = totalCourses,
            PendingApprovals = pendingApprovals,
            TotalEnrollments = totalEnrollments,
            PendingCourses = pendingCourses,
            RecentUsers = recentUsers
        };
        
        return View(viewModel);
    }

    [Route("users")]
    public async Task<IActionResult> ManageUsers(string? role, string? search)
    {
        var users = _userManager.Users.AsQueryable();
        
        if (!string.IsNullOrEmpty(search))
        {
            users = users.Where(u => u.FirstName.Contains(search) || 
                                   u.LastName.Contains(search) || 
                                   u.Email!.Contains(search));
        }
        
        var userList = await users.ToListAsync();
        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in userList)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Student";
            
            if (string.IsNullOrEmpty(role) || userRole == role)
            {
                userViewModels.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    Role = userRole,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                });
            }
        }

        ViewBag.SelectedRole = role;
        ViewBag.Search = search;
        ViewBag.Roles = new[] { "Admin", "Instructor", "Student" };
        return View(userViewModels);
    }

    [HttpPost]
    [Route("users/add")]
    public async Task<IActionResult> AddUser(string firstName, string lastName, string email, string role, string password)
    {
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || 
            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(password))
        {
            TempData["Error"] = "All fields are required.";
            return RedirectToAction(nameof(ManageUsers));
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = "User created successfully.";
        }
        else
        {
            TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost]
    [Route("users/edit")]
    public async Task<IActionResult> EditUser(string id, string firstName, string lastName, string email, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(ManageUsers));
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.UserName = email;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = "User updated successfully.";
        }
        else
        {
            TempData["Error"] = "Failed to update user.";
        }

        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost]
    [Route("users/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = $"User {(user.IsActive ? "activated" : "blocked")} successfully.";
        }
        return RedirectToAction(nameof(ManageUsers));
    }

    [Route("courses/approval")]
    public async Task<IActionResult> ApproveCourses()
    {
        var pendingCourses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Where(c => !c.IsApproved && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(pendingCourses);
    }

    [HttpPost]
    [Route("courses/approve/{id}")]
    public async Task<IActionResult> ApproveCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            course.IsApproved = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course approved successfully.";
        }
        return RedirectToAction(nameof(ApproveCourses));
    }

    [HttpPost]
    [Route("courses/reject/{id}")]
    public async Task<IActionResult> RejectCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            course.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course rejected successfully.";
        }
        return RedirectToAction(nameof(ApproveCourses));
    }

    [Route("categories")]
    public async Task<IActionResult> ManageCategories()
    {
        var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(categories);
    }

    [HttpPost]
    [Route("categories/create")]
    public async Task<IActionResult> CreateCategory(string name, string description)
    {
        if (string.IsNullOrEmpty(name))
        {
            TempData["Error"] = "Category name is required.";
            return RedirectToAction(nameof(ManageCategories));
        }

        var category = new Category
        {
            Name = name,
            Description = description ?? string.Empty
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category created successfully.";
        return RedirectToAction(nameof(ManageCategories));
    }

    [HttpPost]
    [Route("categories/edit")]
    public async Task<IActionResult> EditCategory(int id, string name, string description)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            TempData["Error"] = "Category not found.";
            return RedirectToAction(nameof(ManageCategories));
        }

        category.Name = name;
        category.Description = description ?? string.Empty;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category updated successfully.";
        return RedirectToAction(nameof(ManageCategories));
    }

    [HttpPost]
    [Route("categories/delete/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.Include(c => c.Courses).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            TempData["Error"] = "Category not found.";
            return RedirectToAction(nameof(ManageCategories));
        }

        if (category.Courses.Any())
        {
            TempData["Error"] = "Cannot delete category with existing courses.";
            return RedirectToAction(nameof(ManageCategories));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Category deleted successfully.";
        return RedirectToAction(nameof(ManageCategories));
    }

    [Route("announcements")]
    public async Task<IActionResult> ManageAnnouncements()
    {
        var announcements = await _context.Announcements
            .Include(a => a.CreatedBy)
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
        return View(announcements);
    }

    [HttpPost]
    [Route("announcements/create")]
    public async Task<IActionResult> CreateAnnouncement(string title, string content, bool isPinned = false)
    {
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
        {
            TempData["Error"] = "Title and content are required.";
            return RedirectToAction(nameof(ManageAnnouncements));
        }

        var announcement = new Announcement
        {
            Title = title,
            Content = content,
            IsPinned = isPinned,
            CreatedById = _userManager.GetUserId(User)!
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Announcement created successfully.";
        return RedirectToAction(nameof(ManageAnnouncements));
    }

    [HttpPost]
    [Route("announcements/edit")]
    public async Task<IActionResult> EditAnnouncement(int id, string title, string content, bool isPinned = false)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            TempData["Error"] = "Announcement not found.";
            return RedirectToAction(nameof(ManageAnnouncements));
        }

        announcement.Title = title;
        announcement.Content = content;
        announcement.IsPinned = isPinned;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Announcement updated successfully.";
        return RedirectToAction(nameof(ManageAnnouncements));
    }

    [HttpPost]
    [Route("announcements/toggle/{id}")]
    public async Task<IActionResult> ToggleAnnouncement(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement != null)
        {
            announcement.IsActive = !announcement.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Announcement {(announcement.IsActive ? "activated" : "deactivated")} successfully.";
        }
        return RedirectToAction(nameof(ManageAnnouncements));
    }

    [HttpPost]
    [Route("announcements/delete/{id}")]
    public async Task<IActionResult> DeleteAnnouncement(int id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement != null)
        {
            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Announcement deleted successfully.";
        }
        return RedirectToAction(nameof(ManageAnnouncements));
    }

    [Route("export/users")]
    public async Task<IActionResult> ExportUsers(string format = "csv")
    {
        var users = await _userManager.Users.ToListAsync();
        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserManagementViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? "Student",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        var csv = new StringBuilder();
        csv.AppendLine("FirstName,LastName,Email,Role,IsActive,CreatedAt");
        
        foreach (var user in userViewModels)
        {
            csv.AppendLine($"{user.FirstName},{user.LastName},{user.Email},{user.Role},{user.IsActive},{user.CreatedAt:yyyy-MM-dd}");
        }
        
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"users-{DateTime.Now:yyyyMMdd}.csv");
    }

    [Route("export/courses")]
    public async Task<IActionResult> ExportCourses(string format = "csv")
    {
        var courses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Enrollments)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Title,Instructor,Category,Price,Enrollments,IsApproved,CreatedAt");
        
        foreach (var course in courses)
        {
            csv.AppendLine($"{course.Title},{course.Instructor.FirstName} {course.Instructor.LastName},{course.Category.Name},{course.Price},{course.Enrollments.Count},{course.IsApproved},{course.CreatedAt:yyyy-MM-dd}");
        }
        
        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"courses-{DateTime.Now:yyyyMMdd}.csv");
    }

    [Route("test")]
    public IActionResult Test()
    {
        return View();
    }
}