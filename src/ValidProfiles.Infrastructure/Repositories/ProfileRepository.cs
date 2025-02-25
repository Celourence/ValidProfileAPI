using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Domain;

namespace ValidProfiles.Infrastructure.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly List<Profile> _profiles = new();

    public IEnumerable<Profile> GetProfiles() => _profiles;
    public void AddProfile(Profile profile) => _profiles.Add(profile);
}
