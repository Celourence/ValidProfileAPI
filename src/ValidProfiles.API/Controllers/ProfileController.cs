using Microsoft.AspNetCore.Mvc;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;

namespace ValidProfiles.API.Controllers;

[ApiController]
[Route("api/v1/profiles")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IProfileService profileService, ILogger<ProfileController> logger) => 
        (_profileService, _logger) = (profileService, logger);

    /// <response code="200">Lista de perfis com seus parâmetros</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProfileResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfilesAsync()
    {
        _logger.LogInformation("Obtendo todos os perfis");
        var response = await _profileService.GetProfilesAsync();
        return Ok(response.Profiles);
    }

    /// <param name="name">Nome do perfil a ser buscado</param>
    /// <response code="200">Perfil encontrado com sucesso</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfileByNameAsync(string name)
    {
        _logger.LogInformation($"Buscando perfil: {name}");
        var profile = await _profileService.GetProfileByNameAsync(name);
        return Ok(profile);
    }

    /// <param name="profile">Dados do perfil a ser adicionado</param>
    /// <response code="201">Perfil criado com sucesso</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProfileAsync([FromBody] ProfileDto profile)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        _logger.LogInformation($"Adicionando perfil: {profile.Name}");

        var response = await _profileService.AddProfileAsync(new Profile
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        });
        
        return Created($"api/v1/profiles/{response.Name}", response);
    }

    /// <param name="name">Nome do perfil a ser atualizado</param>
    /// <param name="profileUpdate">Novos parâmetros para o perfil</param>
    /// <response code="200">Perfil atualizado com sucesso</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{name}")]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfileAsync(string name, [FromBody] ProfileUpdateDto profileUpdate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        _logger.LogInformation($"Atualizando perfil: {name}");

        var response = await _profileService.UpdateProfileAsync(name, profileUpdate.Parameters);
        
        return Ok(response);
    }

    /// <param name="name">Nome do perfil a ser excluído</param>
    /// <response code="204">Perfil excluído com sucesso</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProfileAsync(string name)
    {
        _logger.LogInformation($"Excluindo perfil: {name}");

        await _profileService.DeleteProfileAsync(name);
        
        return NoContent();
    }

    /// <param name="name">Nome do perfil a ser validado</param>
    /// <param name="request">Lista de ações a serem validadas</param>
    /// <response code="200">Resultado da validação das permissões</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("{name}/validate")]
    [ProducesResponseType(typeof(ValidationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateProfilePermissionsAsync(string name, [FromBody] ValidationRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        _logger.LogInformation($"Validando permissões para o perfil: {name}");

        var response = await _profileService.ValidateProfilePermissionsAsync(name, request.Actions);
        
        return Ok(response);
    }
}