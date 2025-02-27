using Microsoft.AspNetCore.Mvc;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;

namespace ValidProfiles.API.Controllers;

[ApiController]
[Route("api/profiles")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IProfileService profileService, ILogger<ProfileController> logger) => 
        (_profileService, _logger) = (profileService, logger);

    /// <summary>
    /// Obtém todos os perfis e seus parâmetros
    /// </summary>
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

    /// <summary>
    /// Adiciona um novo perfil com seus parâmetros
    /// </summary>
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

        profile.Parameters ??= new Dictionary<string, string>();
        
        var response = await _profileService.AddProfileAsync(new Profile
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        });
        
        return Created($"api/profiles/{response.Name}", response);
    }
}