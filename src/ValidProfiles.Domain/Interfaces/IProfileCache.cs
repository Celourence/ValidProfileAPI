namespace ValidProfiles.Domain.Interfaces
{
    public interface IProfileCache
    {
        Task<ProfileParameter?> GetAsync(string profileName);
        Task SetAsync(string profileName, ProfileParameter profile);
        Task<Dictionary<string, ProfileParameter>> GetAllAsync();
        Task SetAllAsync(Dictionary<string, ProfileParameter> profiles);
        Task RemoveAsync(string profileName);
        Task ClearAsync();
    }
} 