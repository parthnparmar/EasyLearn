using EasyLearn.Models;
using EasyLearn.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EasyLearn.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IProfileService _profileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(IProfileService profileService, UserManager<ApplicationUser> userManager)
    {
        _profileService = profileService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = _userManager.GetUserId(User)!;
        var profile = await _profileService.GetOrCreateProfileAsync(userId);
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserProfile model)
    {
        var userId = _userManager.GetUserId(User)!;
        var profile = await _profileService.GetOrCreateProfileAsync(userId);
        
        profile.Bio = model.Bio;
        profile.PhoneNumber = model.PhoneNumber;
        profile.Address = model.Address;
        profile.City = model.City;
        profile.Country = model.Country;
        
        await _profileService.UpdateProfileAsync(profile);
        
        TempData["Success"] = "Profile updated successfully!";
        
        var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId) ?? new ApplicationUser());
        return roles.FirstOrDefault() switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "Instructor" => RedirectToAction("Index", "Instructor"),
            _ => RedirectToAction("Index", "Student")
        };
    }
}

public class ProfileEditViewModel
{
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
}