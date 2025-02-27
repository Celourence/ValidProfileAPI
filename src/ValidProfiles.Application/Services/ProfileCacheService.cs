using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ValidProfiles.Application.Services
{
    public class ProfileCacheService : Application.Interfaces.IProfileCacheService
    {
        private readonly IProfileCache _cache;
        private readonly IProfileRepository _repository;
        private readonly ILogger<ProfileCacheService> _logger;
        private readonly IProfileService _profileService;

        public ProfileCacheService(IProfileCache cache, IProfileRepository repository, ILogger<ProfileCacheService> logger, IProfileService profileService) => 
            (_cache, _repository, _logger, _profileService) = (cache, repository, logger, profileService);

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

        public async Task<ProfileParameter?> AddProfileParameterAsync(ProfileParameter profileParameter)
        {
            try
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
                
                return profileParameter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar parâmetro de perfil {ProfileName}", profileParameter.ProfileName);
                return null;
            }
        }
        
        public async Task<ProfileParameter> SetProfileParameterAsync(ProfileParameter profile)
        {
            _logger.LogDebug("Definindo parâmetro de perfil {ProfileName} no cache", profile.ProfileName);
            
            await _cache.SetAsync(profile.ProfileName, profile);
            
            var allProfiles = await _cache.GetAllAsync();
            allProfiles[profile.ProfileName] = profile;
            await _cache.SetAllAsync(allProfiles);
            
            _logger.LogInformation("Parâmetro de perfil {ProfileName} definido com sucesso no cache", profile.ProfileName);
            
            return profile;
        }
        
        public async Task<bool> RemoveProfileParameterAsync(string profileName)
        {
            _logger.LogDebug("Removendo parâmetro de perfil {ProfileName} do cache", profileName);
            
            try
            {
                await _cache.RemoveAsync(profileName);
                
                var allProfiles = await _cache.GetAllAsync();
                if (allProfiles.ContainsKey(profileName))
                {
                    allProfiles.Remove(profileName);
                    await _cache.SetAllAsync(allProfiles);
                }
                
                _logger.LogInformation("Parâmetro de perfil {ProfileName} removido com sucesso do cache", profileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover parâmetro de perfil {ProfileName} do cache", profileName);
                return false;
            }
        }

        public Task ClearCacheAsync() => _cache.ClearAsync();

        public async Task<ValidationResponseDto> ValidateProfilePermissionsAsync(string name, List<string> actions)
        {
            _logger.LogDebug("Validando permissões para o perfil {ProfileName} com cache", name);
            
            // Tentar obter do cache
            var profile = await _cache.GetAsync(name);
            
            // Se não estiver no cache, buscar do repositório
            if (profile == null)
            {
                _logger.LogDebug("Perfil {ProfileName} não encontrado no cache, buscando do repositório", name);
                
                // Chamar o ProfileService para validar
                return await _profileService.ValidateProfilePermissionsAsync(name, actions);
            }
            
            // Perfil encontrado no cache, proceder com a validação
            var response = new ValidationResponseDto();
            
            foreach (var action in actions)
            {
                _logger.LogDebug("Validando permissão '{Action}' para o perfil {ProfileName} (do cache)", action, name);
                
                // Usar switch para determinar o resultado da validação de forma mais elegante
                response.Results[action] = profile.Parameters.TryGetValue(action, out bool allowed) switch
                {
                    true when allowed => "Permitido",
                    true when !allowed => "Negado",
                    _ => "Não definido"
                };
                
                _logger.LogDebug("Resultado da permissão '{Action}' para o perfil {ProfileName} (do cache): {Result}", 
                    action, name, response.Results[action]);
            }
            
            _logger.LogInformation("Validação de permissões concluída para o perfil {ProfileName} (do cache)", name);
            return response;
        }
    }
} 