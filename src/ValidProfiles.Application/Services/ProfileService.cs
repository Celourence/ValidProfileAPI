using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository) => 
        _profileRepository = profileRepository;

    public async Task<ProfilesResponseDto> GetProfilesAsync()
    {
        var profiles = await _profileRepository.GetProfilesAsync();
        return new ProfilesResponseDto
        {
            Profiles = profiles.Select(p => new ProfileResponseDto 
            {
                Name = p.Name,
                Parameters = p.Parameters ?? new Dictionary<string, string>()
            }).ToList()
        };
    }

    public async Task<ProfileResponseDto> AddProfileAsync(Profile profile)
    {
        profile.Parameters ??= new Dictionary<string, string>();
        await _profileRepository.AddProfileAsync(profile);
        
        return new ProfileResponseDto
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        };
    }
}

