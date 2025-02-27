using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Application.Services
{
    /// <summary>
    /// Implementação do serviço de cache de perfis
    /// </summary>
    public class ProfileCacheService : IProfileCacheService
    {
        private readonly IProfileCache _cache;
        private readonly IProfileRepository _repository;
        private readonly ILogger<ProfileCacheService> _logger;
        private readonly IProfileService _profileService;

        /// <summary>
        /// Construtor do serviço de cache de perfis
        /// </summary>
        /// <param name="cache">Cache de perfis</param>
        /// <param name="repository">Repositório de perfis</param>
        /// <param name="logger">Logger para registro de atividades</param>
        /// <param name="profileService">Serviço de perfis</param>
        public ProfileCacheService(
            IProfileCache cache, 
            IProfileRepository repository, 
            ILogger<ProfileCacheService> logger,
            IProfileService profileService)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        }

        /// <inheritdoc/>
        public async Task<ProfileResponseDto> GetProfileAsync(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentException("Profile name cannot be null or empty", nameof(profileName));
            }

            // Tentar obter do cache primeiro
            var cachedParameter = await _cache.GetAsync(profileName);
            if (cachedParameter != null)
            {
                _logger.LogDebug("Perfil {ProfileName} encontrado no cache", profileName);
                return MapToProfileResponseDto(cachedParameter);
            }

            // Se não estiver no cache, obter do repositório e salvar no cache
            _logger.LogDebug("Perfil {ProfileName} não encontrado no cache, buscando no repositório", profileName);
            var profile = await _repository.GetProfileByNameAsync(profileName);
            if (profile != null)
            {
                var parameter = new ProfileParameter
                {
                    ProfileName = profile.Name,
                    Parameters = profile.Parameters
                };
                await _cache.SetAsync(profileName, parameter);
                _logger.LogDebug("Perfil {ProfileName} adicionado ao cache", profileName);
                return MapToProfileResponseDto(parameter);
            }

            // Ao invés de retornar null, vamos lançar uma exceção
            throw new KeyNotFoundException($"Profile with name '{profileName}' not found");
        }

        /// <inheritdoc/>
        public async Task SetProfileAsync(ProfileResponseDto profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var parameter = new ProfileParameter
            {
                ProfileName = profile.Name,
                Parameters = profile.Parameters
            };

            await _cache.SetAsync(profile.Name, parameter);
            _logger.LogDebug("Perfil {ProfileName} atualizado no cache", profile.Name);
        }

        /// <inheritdoc/>
        public async Task RemoveProfileAsync(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentException("Profile name cannot be null or empty", nameof(profileName));
            }

            await _cache.RemoveAsync(profileName);
            _logger.LogDebug("Perfil {ProfileName} removido do cache", profileName);
        }

        /// <inheritdoc/>
        public async Task RefreshCacheAsync()
        {
            _logger.LogInformation("Iniciando atualização completa do cache de perfis");
            var profiles = await _repository.GetProfilesAsync();
            int count = 0;

            foreach (var profile in profiles)
            {
                var parameter = new ProfileParameter
                {
                    ProfileName = profile.Name,
                    Parameters = profile.Parameters
                };
                
                await _cache.SetAsync(profile.Name, parameter);
                count++;
            }

            _logger.LogInformation("Atualização do cache de perfis concluída. Total de {Count} perfis atualizados", count);
        }

        /// <inheritdoc/>
        public async Task<ProfileParameter> GetProfileParameterAsync(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentException("Profile name cannot be null or empty", nameof(profileName));
            }

            var parameter = await _cache.GetAsync(profileName);
            if (parameter == null)
            {
                _logger.LogDebug("Parâmetro de perfil {ProfileName} não encontrado no cache", profileName);
                var profile = await _repository.GetProfileByNameAsync(profileName);
                if (profile != null)
                {
                    parameter = new ProfileParameter
                    {
                        ProfileName = profile.Name,
                        Parameters = profile.Parameters
                    };
                    await _cache.SetAsync(profileName, parameter);
                    _logger.LogDebug("Parâmetro de perfil {ProfileName} adicionado ao cache", profileName);
                    return parameter;
                }
                
                throw new KeyNotFoundException($"Profile with name '{profileName}' not found");
            }
            
            return parameter;
        }

        /// <inheritdoc/>
        public async Task<ProfileParameter> SetProfileParameterAsync(ProfileParameter profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            await _cache.SetAsync(profile.ProfileName, profile);
            return profile;
        }

        /// <inheritdoc/>
        public async Task<ProfileParameter> AddProfileParameterAsync(ProfileParameter profileParameter)
        {
            try
            {
                await _repository.AddProfileAsync(new Profile 
                { 
                    Name = profileParameter.ProfileName, 
                    Parameters = profileParameter.Parameters 
                });
                
                await _cache.SetAsync(profileParameter.ProfileName, profileParameter);
                
                _logger.LogInformation("Parâmetro de perfil {ProfileName} adicionado com sucesso ao cache", profileParameter.ProfileName);
                return profileParameter;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar parâmetro de perfil {ProfileName}", profileParameter.ProfileName);
                return profileParameter; // Retornar objeto não nulo para evitar warning
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveProfileParameterAsync(string profileName)
        {
            _logger.LogDebug("Removendo parâmetro de perfil {ProfileName} do cache", profileName);
            
            try
            {
                await _cache.RemoveAsync(profileName);
                _logger.LogInformation("Parâmetro de perfil {ProfileName} removido com sucesso do cache", profileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover parâmetro de perfil {ProfileName} do cache", profileName);
                return false;
            }
        }

        /// <inheritdoc/>
        public Task ClearCacheAsync()
        {
            _logger.LogInformation("Limpando o cache de perfis");
            return _cache.ClearAsync();
        }

        /// <inheritdoc/>
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
            var response = new ValidationResponseDto
            {
                ProfileName = name
            };
            
            foreach (var action in actions)
            {
                _logger.LogDebug("Validando permissão '{Action}' para o perfil {ProfileName} (do cache)", action, name);
                
                // Usar switch para determinar o resultado da validação de forma mais elegante
                if (profile.Parameters.TryGetValue(action, out bool allowed))
                {
                    response.Results[action] = allowed ? "Allowed" : "Denied";
                }
                else
                {
                    response.Results[action] = "Undefined";
                }
                
                _logger.LogDebug("Resultado da permissão '{Action}' para o perfil {ProfileName} (do cache): {Result}", 
                    action, name, response.Results[action]);
            }
            
            _logger.LogInformation("Validação de permissões concluída para o perfil {ProfileName} (do cache)", name);
            return response;
        }

        /// <summary>
        /// Converte um ProfileParameter para ProfileResponseDto
        /// </summary>
        private ProfileResponseDto MapToProfileResponseDto(ProfileParameter parameter)
        {
            return new ProfileResponseDto
            {
                Name = parameter.ProfileName,
                Parameters = parameter.Parameters ?? new Dictionary<string, bool>()
            };
        }
    }
} 