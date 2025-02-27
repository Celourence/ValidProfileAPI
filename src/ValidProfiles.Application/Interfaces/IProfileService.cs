using ValidProfiles.Application.DTOs;
using ValidProfiles.Domain;

namespace ValidProfiles.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfilesResponseDto> GetProfilesAsync();
        Task<ProfileResponseDto> GetProfileByNameAsync(string name);
        Task<ProfileResponseDto> AddProfileAsync(Profile profile);
        Task<ProfileResponseDto> UpdateProfileAsync(string name, Dictionary<string, bool> parameters);
        Task DeleteProfileAsync(string name);
    }
} 