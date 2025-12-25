using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EasyLearn.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLearn.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var featuredCourses = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Where(c => c.IsApproved && c.IsActive)
            .Take(6)
            .ToListAsync();

        ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        return View("IndexModern", featuredCourses);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}