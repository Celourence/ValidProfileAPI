using ValidProfiles.Application.DTOs;
using ValidProfiles.Domain;

namespace ValidProfiles.Application.Interfaces
{
    public interface IProfileCacheService
    {
        Task<ProfileParameter?> GetProfileParameterAsync(string profileName);
        Task<ProfileParameter> SetProfileParameterAsync(ProfileParameter profile);
        Task<ProfileParameter?> AddProfileParameterAsync(ProfileParameter profile);
        Task<bool> RemoveProfileParameterAsync(string profileName);
        Task ClearCacheAsync();
        Task<ValidationResponseDto> ValidateProfilePermissionsAsync(string name, List<string> actions);
    }
} 