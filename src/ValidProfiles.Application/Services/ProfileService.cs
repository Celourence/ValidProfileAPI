using ValidProfiles.Application.DTOs;
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
        _logger.LogDebug("Getting all profiles");
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
        _logger.LogDebug("Finding profile by name: {ProfileName}", name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Attempt to find profile with empty name");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
        
        var profile = await _profileRepository.GetProfileByNameAsync(name);
        
        if (profile == null)
        {
            _logger.LogWarning(LogMessages.ProfileService.ProfileNotFound, name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        _logger.LogInformation("Profile found: {ProfileName}", name);
        
        return new ProfileResponseDto
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        };
    }

    public async Task<ProfileResponseDto> AddProfileAsync(Profile profile)
    {
        _logger.LogDebug(LogMessages.ProfileService.StartingProfileCreation, profile.Name);
        
        // Validação do nome já é feita pelo ProfileDto na API
        
        var existingProfiles = await _profileRepository.GetProfilesAsync();
        if (existingProfiles.Any(p => p.Name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Attempt to add profile with existing name: {ProfileName}", profile.Name);
            throw new ConflictException(ErrorMessages.Profile.ProfileAlreadyExists);
        }
        
        // Validação dos parâmetros já é feita pelo ProfileDto na API, mas verificamos novamente
        // para casos onde a API não é usada diretamente
        if (profile.Parameters == null || profile.Parameters.Count == 0)
        {
            _logger.LogWarning("Attempt to add profile without parameters: {ProfileName}", profile.Name);
            throw new BadRequestException("Parameter list cannot be empty");
        }

        foreach (var param in profile.Parameters)
        {
            if (string.IsNullOrWhiteSpace(param.Key))
            {
                _logger.LogWarning("Parameter with empty name in profile {ProfileName}", profile.Name);
                throw new BadRequestException(ErrorMessages.Profile.EmptyParameterName);
            }
        }
        
        _logger.LogInformation("Adding new profile: {ProfileName}", profile.Name);
        
        await _profileRepository.AddProfileAsync(profile);
        
        _logger.LogInformation(LogMessages.ProfileService.ProfileCreated, profile.Name);
        
        return new ProfileResponseDto
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        };
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(string name, Dictionary<string, bool> parameters)
    {
        _logger.LogDebug(LogMessages.ProfileService.StartingProfileUpdate, name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Attempt to update profile with empty name");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
                
        // Validação de parameters já é feita pelo ProfileUpdateDto
        if (parameters == null || parameters.Count == 0)
        {
            _logger.LogWarning("Attempt to update profile with empty parameters: {ProfileName}", name);
            throw new BadRequestException("Parameter list cannot be empty");
        }

        var profile = await _profileRepository.GetProfileByNameAsync(name);
        if (profile == null)
        {
            _logger.LogWarning("Attempt to update non-existing profile: {ProfileName}", name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        foreach (var param in parameters)
        {
            if (string.IsNullOrWhiteSpace(param.Key))
            {
                _logger.LogWarning("Parameter with empty name in profile update {ProfileName}", name);
                throw new BadRequestException(ErrorMessages.Profile.EmptyParameterName);
            }
        }
        
        _logger.LogInformation("Updating profile parameters: {ProfileName}", name);
        
        profile.Parameters = parameters;
        
        await _profileRepository.UpdateProfileAsync(profile);
        
        _logger.LogInformation(LogMessages.ProfileService.ProfileUpdated, name);
        
        return new ProfileResponseDto
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        };
    }
    
    public async Task DeleteProfileAsync(string name)
    {
        _logger.LogDebug(LogMessages.ProfileService.StartingProfileDeletion, name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Attempt to delete profile with empty name");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
                
        var profile = await _profileRepository.GetProfileByNameAsync(name);
        if (profile == null)
        {
            _logger.LogWarning("Attempt to remove non-existing profile: {ProfileName}", name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        _logger.LogInformation("Removing profile: {ProfileName}", name);
        
        await _profileRepository.DeleteProfileAsync(name);
        
        _logger.LogInformation(LogMessages.ProfileService.ProfileDeleted, name);
    }

    public async Task<ValidationResponseDto> ValidateProfilePermissionsAsync(string name, List<string> actions)
    {
        _logger.LogInformation(LogMessages.ProfileService.ValidatingPermissions, name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Attempt to validate permissions with empty profile name");
            throw new BadRequestException(ErrorMessages.Profile.InvalidProfileName);
        }
       
        if (actions == null || actions.Count == 0)
        {
            _logger.LogWarning("Empty action list for profile validation {ProfileName}", name);
            throw new BadRequestException("Action list cannot be empty");
        }
        
        var profile = await _profileRepository.GetProfileByNameAsync(name);
        if (profile == null)
        {
            _logger.LogWarning("Attempt to validate permissions for non-existing profile: {ProfileName}", name);
            throw new NotFoundException(ErrorMessages.Profile.ProfileNotFound);
        }
        
        var response = new ValidationResponseDto
        {
            ProfileName = name
        };
        
        foreach (var action in actions)
        {
            _logger.LogDebug("Validating permission '{Action}' for profile {ProfileName}", action, name);
            
            if (profile.Parameters.TryGetValue(action, out bool allowed))
            {
                response.Results[action] = allowed ? "Allowed" : "Denied";
            }
            else
            {
                response.Results[action] = "Undefined";
            }
            
            _logger.LogDebug("Permission result '{Action}' for profile {ProfileName}: {Result}", 
                action, name, response.Results[action]);
        }
        
        _logger.LogInformation(LogMessages.ProfileService.PermissionValidationResult, name, response.Results.Count);
        return response;
    }
}

