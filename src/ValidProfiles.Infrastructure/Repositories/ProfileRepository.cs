using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Infrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly Dictionary<string, ProfileParameter> _profiles = new(StringComparer.OrdinalIgnoreCase);

    public Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        var profiles = _profiles.Values.Select(p => new Profile
        {
            Name = p.ProfileName,
            Parameters = p.Parameters
        });
        
        return Task.FromResult<IEnumerable<Profile>>(profiles);
    }
    
    public Task<Profile?> GetProfileByNameAsync(string name)
    {
        if (_profiles.TryGetValue(name, out var profileParam))
        {
            return Task.FromResult<Profile?>(new Profile
            {
                Name = profileParam.ProfileName,
                Parameters = profileParam.Parameters
            });
        }
        
        return Task.FromResult<Profile?>(null);
    }

    public Task AddProfileAsync(Profile profile)
    {
        var profileParam = new ProfileParameter
        {
            ProfileName = profile.Name,
            Parameters = profile.Parameters
        };
        
        _profiles[profile.Name] = profileParam;
        return Task.CompletedTask;
    }
    
    public Task<Profile> UpdateProfileAsync(Profile profile)
    {
        if (_profiles.TryGetValue(profile.Name, out var existingProfile))
        {
            existingProfile.Parameters = profile.Parameters;
            
            return Task.FromResult(new Profile
            {
                Name = existingProfile.ProfileName,
                Parameters = existingProfile.Parameters
            });
        }
        
        var profileParam = new ProfileParameter
        {
            ProfileName = profile.Name,
            Parameters = profile.Parameters
        };
        
        _profiles[profile.Name] = profileParam;
        
        return Task.FromResult(profile);
    }
    
    public Task DeleteProfileAsync(string name)
    {
        _profiles.Remove(name);
        return Task.CompletedTask;
    }
}
