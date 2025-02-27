﻿using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Constants;
using ValidProfiles.Domain.Exceptions;
using ValidProfiles.Domain.Interfaces;
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

    public async Task<ProfileResponseDto> UpdateProfileAsync(string name, Dictionary<string, bool> parameters)
    {
        _logger.LogDebug("Iniciando atualização do perfil {ProfileName}", name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Tentativa de atualizar perfil com nome vazio");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
        
        if (parameters == null || parameters.Count == 0)
        {
            _logger.LogWarning("Tentativa de atualizar perfil com parâmetros vazios: {ProfileName}", name);
            throw new BadRequestException("A lista de parâmetros não pode estar vazia");
        }
        
        // Verificar se o perfil existe
        var profile = await _profileRepository.GetProfileByNameAsync(name);
        if (profile == null)
        {
            _logger.LogWarning("Tentativa de atualizar perfil inexistente: {ProfileName}", name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        // Validar parâmetros
        foreach (var param in parameters)
        {
            if (string.IsNullOrWhiteSpace(param.Key))
            {
                _logger.LogWarning("Parâmetro com nome vazio na atualização do perfil {ProfileName}", name);
                throw new BadRequestException(ErrorMessages.Profile.EmptyParameterName);
            }
        }
        
        _logger.LogInformation("Atualizando parâmetros do perfil: {ProfileName}", name);
        
        // Atualizar os parâmetros
        profile.Parameters = parameters;
        
        await _profileRepository.UpdateProfileAsync(profile);
        
        _logger.LogInformation("Perfil atualizado com sucesso: {ProfileName}", name);
        
        return new ProfileResponseDto
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        };
    }
    
    public async Task DeleteProfileAsync(string name)
    {
        _logger.LogDebug("Iniciando remoção do perfil {ProfileName}", name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Tentativa de remover perfil com nome vazio");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
        
        // Verificar se o perfil existe
        var profile = await _profileRepository.GetProfileByNameAsync(name);
        if (profile == null)
        {
            _logger.LogWarning("Tentativa de remover perfil inexistente: {ProfileName}", name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        _logger.LogInformation("Removendo perfil: {ProfileName}", name);
        
        await _profileRepository.DeleteProfileAsync(name);
        
        _logger.LogInformation("Perfil removido com sucesso: {ProfileName}", name);
    }

    public async Task<ValidationResponseDto> ValidateProfilePermissionsAsync(string name, List<string> actions)
    {
        _logger.LogDebug("Validando permissões para o perfil {ProfileName}", name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Tentativa de validar permissões para perfil com nome vazio");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
        
        if (actions == null || actions.Count == 0)
        {
            _logger.LogWarning("Lista de ações vazia para validação do perfil {ProfileName}", name);
            throw new BadRequestException("A lista de ações não pode estar vazia");
        }
        
        // Verificar se o perfil existe
        var profile = await _profileRepository.GetProfileByNameAsync(name);
        if (profile == null)
        {
            _logger.LogWarning("Tentativa de validar permissões para perfil inexistente: {ProfileName}", name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        var response = new ValidationResponseDto();
        
        foreach (var action in actions)
        {
            _logger.LogDebug("Validando permissão '{Action}' para o perfil {ProfileName}", action, name);
            
            // Usar switch para determinar o resultado da validação de forma mais elegante
            response.Results[action] = profile.Parameters.TryGetValue(action, out bool allowed) switch
            {
                true when allowed => "Permitido",
                true when !allowed => "Negado",
                _ => "Não definido"
            };
            
            _logger.LogDebug("Resultado da permissão '{Action}' para o perfil {ProfileName}: {Result}", 
                action, name, response.Results[action]);
        }
        
        _logger.LogInformation("Validação de permissões concluída para o perfil {ProfileName}", name);
        return response;
    }
}

