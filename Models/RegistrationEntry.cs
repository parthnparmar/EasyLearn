using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class RegistrationEntry
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    public DateTime RegistrationTime { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    public bool IsEmailVerified { get; set; }
    
    public string? Role { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; }
}