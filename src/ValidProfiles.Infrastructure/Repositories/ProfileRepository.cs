using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Infrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly List<Profile> _profiles = new();

    public Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        return Task.FromResult<IEnumerable<Profile>>(_profiles);
    }
    
    public Task<Profile?> GetProfileByNameAsync(string name)
    {
        return Task.FromResult(_profiles.FirstOrDefault(p => 
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddProfileAsync(Profile profile)
    {
        _profiles.Add(profile);
        return Task.CompletedTask;
    }
    
    public Task<Profile> UpdateProfileAsync(Profile profile)
    {
        var existingProfile = _profiles.FirstOrDefault(p => 
            p.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase));
            
        if (existingProfile != null)
        {
            existingProfile.Parameters = profile.Parameters;
        }
        
        return Task.FromResult(existingProfile!);
    }
    
    public Task DeleteProfileAsync(string name)
    {
        var profile = _profiles.FirstOrDefault(p => 
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
        if (profile != null)
        {
            _profiles.Remove(profile);
        }
        
        return Task.CompletedTask;
    }
}
