using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Constants;
using ValidProfiles.Domain.Exceptions;
using ValidProfiles.Domain.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ValidProfiles.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(IProfileRepository profileRepository, ILogger<ProfileService> logger)
    {
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<ProfilesResponseDto> GetProfilesAsync()
    {
        _logger.LogDebug("Obtendo todos os perfis");
        var profiles = await _profileRepository.GetProfilesAsync();
        return new ProfilesResponseDto
        {
            Profiles = profiles.Select(p => new ProfileResponseDto
            {
                Name = p.Name,
                Parameters = p.Parameters
            }).ToList()
        };
    }

    public async Task<ProfileResponseDto> GetProfileByNameAsync(string name)
    {
        _logger.LogDebug("Buscando perfil por nome: {ProfileName}", name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Tentativa de buscar perfil com nome vazio");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
        
        var profile = await _profileRepository.GetProfileByNameAsync(name);
        
        if (profile == null)
        {
            _logger.LogWarning("Perfil não encontrado: {ProfileName}", name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        _logger.LogInformation("Perfil encontrado: {ProfileName}", name);
        
        return new ProfileResponseDto
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        };
    }

    public async Task<ProfileResponseDto> AddProfileAsync(Profile profile)
    {
        _logger.LogDebug("Iniciando validação do perfil {ProfileName}", profile.Name);
        
        // Validar se o nome do perfil está no formato correto
        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            _logger.LogWarning("Tentativa de adicionar perfil com nome vazio");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
        
        // Verificar se já existe um perfil com o mesmo nome
        var existingProfiles = await _profileRepository.GetProfilesAsync();
        if (existingProfiles.Any(p => p.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Tentativa de adicionar perfil com nome já existente: {ProfileName}", profile.Name);
            throw new ConflictException(ErrorMessages.Profile.ProfileAlreadyExists);
        }
        
        // Validar parâmetros
        // Verificar se há parâmetros (não pode estar vazio)
        if (profile.Parameters == null || profile.Parameters.Count == 0)
        {
            _logger.LogWarning("Tentativa de adicionar perfil sem parâmetros: {ProfileName}", profile.Name);
            throw new BadRequestException("A lista de parâmetros não pode estar vazia");
        }

        foreach (var param in profile.Parameters)
        {
            if (string.IsNullOrWhiteSpace(param.Key))
            {
                _logger.LogWarning("Parâmetro com nome vazio no perfil {ProfileName}", profile.Name);
                throw new BadRequestException(ErrorMessages.Profile.EmptyParameterName);
            }
            
            // Não é mais necessário validar se o valor é booleano, pois o tipo já garante isso
        }
        
        _logger.LogInformation("Adicionando novo perfil: {ProfileName}", profile.Name);
        
        // Garantir que Parameters não seja nulo
        profile.Parameters ??= new();
        
        await _profileRepository.AddProfileAsync(profile);
        
        _logger.LogInformation("Perfil adicionado com sucesso: {ProfileName}", profile.Name);
        
        return new ProfileResponseDto
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        };
    }
}

