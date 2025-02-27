namespace ValidProfiles.Domain.Interfaces
{
    public interface IProfileCacheService
    {
        Task<ProfileParameter?> GetProfileParameterAsync(string profileName);
        Task<Dictionary<string, ProfileParameter>> GetAllProfileParametersAsync();
        Task AddProfileParameterAsync(ProfileParameter profileParameter);
        Task ClearCacheAsync();
    }
} 