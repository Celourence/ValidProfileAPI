using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Application.Services
{
    public class ProfileCacheService : IProfileCacheService
    {
        private readonly IProfileCache _cache;
        private readonly IProfileRepository _repository;

        public ProfileCacheService(IProfileCache cache, IProfileRepository repository) => 
            (_cache, _repository) = (cache, repository);

        public async Task<ProfileParameter?> GetProfileParameterAsync(string profileName)
        {
            var cachedProfile = await _cache.GetAsync(profileName);
            if (cachedProfile != null) 
                return cachedProfile;

            var profiles = await _repository.GetProfilesAsync();
            var profile = profiles.FirstOrDefault(p => p.Name == profileName);
            
            if (profile == null)
                return null;

            var profileParameter = new ProfileParameter
            {
                ProfileName = profile.Name,
                Parameters = profile.Parameters
            };

            await _cache.SetAsync(profileName, profileParameter);
            return profileParameter;
        }

        public async Task<Dictionary<string, ProfileParameter>> GetAllProfileParametersAsync()
        {
            var cachedProfiles = await _cache.GetAllAsync();
            if (cachedProfiles?.Count > 0)
                return cachedProfiles;

            var profiles = await _repository.GetProfilesAsync();
            var profileParameters = profiles.ToDictionary(
                p => p.Name,
                p => new ProfileParameter
                {
                    ProfileName = p.Name,
                    Parameters = p.Parameters ?? new Dictionary<string, bool>()
                });

            if (profileParameters.Count > 0)
                await _cache.SetAllAsync(profileParameters);

            return profileParameters;
        }

        public async Task AddProfileParameterAsync(ProfileParameter profileParameter)
        {
            await _repository.AddProfileAsync(new Profile 
            { 
                Name = profileParameter.ProfileName, 
                Parameters = profileParameter.Parameters 
            });
            
            await _cache.SetAsync(profileParameter.ProfileName, profileParameter);

            var allProfiles = await _cache.GetAllAsync();
            allProfiles[profileParameter.ProfileName] = profileParameter;
            await _cache.SetAllAsync(allProfiles);
        }

        public Task ClearCacheAsync() => _cache.ClearAsync();
    }
} 