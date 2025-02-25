using ValidProfiles.Application.DTOs;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Application;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public IEnumerable<Profile> GetProfiles()
    {
        var profiles = _profileRepository.GetProfiles();
        return profiles.Select(p => new Profile 
        {
            Name = p.Name,
            Parameters = p.Parameters
        });
    }

    public void AddProfile(Profile profile)
    {
        _profileRepository.AddProfile(profile);
    }
}

