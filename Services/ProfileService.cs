using EasyLearn.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyLearn.Services;

public interface IProfileService
{
    Task<UserProfile> GetOrCreateProfileAsync(string userId);
    Task<UserProfile?> GetProfileAsync(string userId);
    Task UpdateProfileAsync(UserProfile profile);
    Task<bool> IsProfileCompleteAsync(string userId);
}

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly string[] _defaultAvatars = {
        "/images/avatars/avatar1.svg",
        "/images/avatars/avatar2.svg", 
        "/images/avatars/avatar3.svg",
        "/images/avatars/avatar4.svg",
        "/images/avatars/avatar5.svg"
    };

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile> GetOrCreateProfileAsync(string userId)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            var random = new Random();
            profile = new UserProfile
            {
                UserId = userId,
                AvatarUrl = _defaultAvatars[random.Next(_defaultAvatars.Length)]
            };
            
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        return profile;
    }

    public async Task<UserProfile?> GetProfileAsync(string userId)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task UpdateProfileAsync(UserProfile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        _context.UserProfiles.Update(profile);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsProfileCompleteAsync(string userId)
    {
        var profile = await GetProfileAsync(userId);
        return profile?.CompletionPercentage == 100;
    }
}