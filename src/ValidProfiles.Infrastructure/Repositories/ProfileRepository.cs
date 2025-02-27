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
}
