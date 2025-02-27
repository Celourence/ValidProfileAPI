using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Infrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly List<Profile> _profiles = new();

    public Task<IEnumerable<Profile>> GetProfilesAsync() => 
        Task.FromResult<IEnumerable<Profile>>(_profiles);
            
    public Task AddProfileAsync(Profile profile)
    {
        _profiles.Add(profile);
        return Task.CompletedTask;
    }
    public async Task<IEnumerable<Profile>> GetProfilesAsync()
    {
        return await Task.FromResult(_profiles);
    }
    
    public async Task<Profile?> GetProfileByNameAsync(string name)
    {
        return await Task.FromResult(_profiles.FirstOrDefault(p => 
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddProfileAsync(Profile profile)
    {
        _profiles.Add(profile);
        return Task.CompletedTask;
    }
}
