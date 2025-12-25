using EasyLearn.Models;
using EasyLearn.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EasyLearn.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IProfileService _profileService;
    private readonly ApplicationDbContext _context;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IProfileService profileService, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _profileService = profileService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register() => View("RegisterModern");

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await EnsureRolesExist();
                var role = !string.IsNullOrEmpty(model.Role) ? model.Role : "Student";
                await _userManager.AddToRoleAsync(user, role);
                await _profileService.GetOrCreateProfileAsync(user.Id);
                
                // Store registration entry
                var registrationEntry = new RegistrationEntry
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RegistrationTime = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
                    IsEmailVerified = false,
                    Role = role
                };
                _context.RegistrationEntries.Add(registrationEntry);
                await _context.SaveChangesAsync();
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                return role switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Instructor" => RedirectToAction("Index", "Instructor"),
                    _ => RedirectToAction("Index", "Student")
                };
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
        return View("RegisterModern", model);
    }

    [HttpGet]
    public IActionResult Login() => View("LoginModern");

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            
            // Only store login entry if user exists
            if (user != null)
            {
                var loginEntry = new LoginEntry
                {
                    UserId = user.Id,
                    Email = model.Email,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
                    IsSuccessful = result.Succeeded,
                    FailureReason = result.Succeeded ? null : "Invalid credentials"
                };
                _context.LoginEntries.Add(loginEntry);
                await _context.SaveChangesAsync();
            }
            
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user!);
                await _profileService.GetOrCreateProfileAsync(user!.Id);
                
                return roles.FirstOrDefault() switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Instructor" => RedirectToAction("Index", "Instructor"),
                    _ => RedirectToAction("Index", "Student")
                };
            }
            ModelState.AddModelError("", "Invalid login attempt.");
        }
        return View("LoginModern", model);
    }

    [HttpPost]
    public async Task<IActionResult> ExternalLogin(string provider, string returnUrl = null!)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null!, string remoteError = null!)
    {
        if (remoteError != null)
        {
            ModelState.AddModelError("", $"Error from external provider: {remoteError}");
            return View("LoginModern");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            var roles = await _userManager.GetRolesAsync(user!);
            await _profileService.GetOrCreateProfileAsync(user!.Id);
            
            return roles.FirstOrDefault() switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Instructor" => RedirectToAction("Index", "Instructor"),
                _ => RedirectToAction("Index", "Student")
            };
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
        var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

        if (email != null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };

                await _userManager.CreateAsync(user);
                await EnsureRolesExist();
                await _userManager.AddToRoleAsync(user, "Student");
                await _profileService.GetOrCreateProfileAsync(user.Id);
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Student");
        }

        return View("LoginModern");
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // In a real application, you would send this token via email
                TempData["ResetToken"] = token;
                TempData["UserId"] = user.Id;
                return RedirectToAction(nameof(ResetPassword));
            }
            ModelState.AddModelError("", "Email not found.");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = TempData["UserId"]?.ToString();
            var token = TempData["ResetToken"]?.ToString();
            
            if (userId != null && token != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Password reset successfully.";
                        return RedirectToAction(nameof(Login));
                    }
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
        }
        return View(model);
    }

    private async Task EnsureRolesExist()
    {
        string[] roles = { "Admin", "Instructor", "Student" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

public class RegisterViewModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
}

public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}