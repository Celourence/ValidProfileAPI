using ValidProfiles.Application.DTOs;
using ValidProfiles.Domain;

namespace ValidProfiles.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfilesResponseDto> GetProfilesAsync();
        Task<ProfileResponseDto> AddProfileAsync(Profile profile);
    }
} 