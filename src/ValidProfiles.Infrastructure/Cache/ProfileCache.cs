using Microsoft.Extensions.Caching.Memory;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Infrastructure.Cache
{
    public class ProfileCache : IProfileCache
    {
        private readonly IMemoryCache _memoryCache;
        private const string PROFILES_KEY = "allprofiles";

        public ProfileCache(IMemoryCache memoryCache) => _memoryCache = memoryCache;
        
        public Task<ProfileParameter?> GetAsync(string profileName) => 
            GetAllAsync().ContinueWith(t => 
                t.Result.TryGetValue(profileName, out var profile) ? profile : null);

        public async Task SetAsync(string profileName, ProfileParameter profile)
        {
            var allProfiles = await GetAllAsync();
            allProfiles[profileName] = profile;
            await SetAllAsync(allProfiles);
        }

        public Task<Dictionary<string, ProfileParameter>> GetAllAsync() =>
            _memoryCache.TryGetValue(PROFILES_KEY, out Dictionary<string, ProfileParameter>? profiles)
                ? Task.FromResult(profiles ?? new Dictionary<string, ProfileParameter>())
                : Task.FromResult(new Dictionary<string, ProfileParameter>());

        public Task SetAllAsync(Dictionary<string, ProfileParameter> profiles)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            };
            
            _memoryCache.Set(PROFILES_KEY, profiles, cacheOptions);
            return Task.CompletedTask;
        }

        public async Task RemoveAsync(string profileName)
        {
            var allProfiles = await GetAllAsync();
            if (allProfiles.ContainsKey(profileName))
            {
                allProfiles.Remove(profileName);
                await SetAllAsync(allProfiles);
            }
        }

        public Task ClearAsync()
        {
            _memoryCache.Remove(PROFILES_KEY);
            return Task.CompletedTask;
        }
    }
} 