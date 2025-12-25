using System.ComponentModel.DataAnnotations;

namespace EasyLearn.Models;

public class LoginEntry
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    public DateTime LoginTime { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
    
    public bool IsSuccessful { get; set; }
    
    public string? FailureReason { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; }
}