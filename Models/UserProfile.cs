namespace EasyLearn.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string AvatarUrl { get; set; } = "/images/avatars/default.svg";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public ApplicationUser User { get; set; } = null!;
    
    public int CompletionPercentage
    {
        get
        {
            int completed = 0;
            int total = 5;
            
            if (!string.IsNullOrEmpty(Bio)) completed++;
            if (!string.IsNullOrEmpty(PhoneNumber)) completed++;
            if (!string.IsNullOrEmpty(Address)) completed++;
            if (!string.IsNullOrEmpty(City)) completed++;
            if (!string.IsNullOrEmpty(Country)) completed++;
            
            return (int)((completed / (double)total) * 100);
        }
    }
}
