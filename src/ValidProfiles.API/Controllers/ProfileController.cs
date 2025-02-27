using Microsoft.AspNetCore.Mvc;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;

namespace ValidProfiles.API.Controllers;

/// <summary>
/// Controller responsável por gerenciar os perfis de acesso
/// </summary>
[ApiController]
[Route("api/v1/profiles")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IProfileService profileService, ILogger<ProfileController> logger) => 
        (_profileService, _logger) = (profileService, logger);

    /// <summary>
    /// Obtém todos os perfis cadastrados
    /// </summary>
    /// <returns>Lista de perfis com seus respectivos parâmetros</returns>
    /// <response code="200">Retorna a lista de perfis cadastrados</response>
    /// <response code="500">Ocorreu um erro no servidor</response>
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
    /// Obtém um perfil pelo nome
    /// </summary>
    /// <param name="name">Nome do perfil a ser consultado</param>
    /// <returns>Detalhes do perfil consultado</returns>
    /// <response code="200">Retorna o perfil encontrado</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Ocorreu um erro no servidor</response>
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

    /// <summary>
    /// Cria um novo perfil
    /// </summary>
    /// <param name="profile">Dados do perfil a ser criado</param>
    /// <returns>Detalhes do perfil criado</returns>
    /// <response code="201">Perfil criado com sucesso</response>
    /// <response code="400">Dados do perfil inválidos</response>
    /// <response code="500">Ocorreu um erro no servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddProfileAsync([FromBody] ProfileDto profile)
    {            
        _logger.LogInformation($"Adicionando perfil: {profile.Name}");

        var response = await _profileService.AddProfileAsync(new Profile
        {
            Name = profile.Name,
            Parameters = profile.Parameters
        });
        
        return Created($"api/v1/profiles/{response.Name}", response);
    }

    /// <summary>
    /// Atualiza um perfil existente
    /// </summary>
    /// <param name="name">Nome do perfil a ser atualizado</param>
    /// <param name="profileUpdate">Novos parâmetros para o perfil</param>
    /// <returns>Detalhes do perfil atualizado</returns>
    /// <response code="200">Perfil atualizado com sucesso</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Dados de atualização inválidos</response>
    /// <response code="500">Ocorreu um erro no servidor</response>
    [HttpPut("{name}")]
    [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfileAsync(string name, [FromBody] ProfileUpdateDto profileUpdate)
    {            
        _logger.LogInformation($"Atualizando perfil: {name}");

        var response = await _profileService.UpdateProfileAsync(name, profileUpdate.Parameters);
        
        return Ok(response);
    }

    /// <summary>
    /// Remove um perfil
    /// </summary>
    /// <param name="name">Nome do perfil a ser removido</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Perfil removido com sucesso</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Ocorreu um erro no servidor</response>
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

    /// <summary>
    /// Valida as permissões de um perfil para ações específicas
    /// </summary>
    /// <param name="name">Nome do perfil a ser validado</param>
    /// <param name="request">Lista de ações a serem validadas</param>
    /// <returns>Resultado da validação para cada ação</returns>
    /// <response code="200">Validação realizada com sucesso</response>
    /// <response code="404">Perfil não encontrado</response>
    /// <response code="400">Requisição inválida</response>
    /// <response code="500">Ocorreu um erro no servidor</response>
    [HttpPost("{name}/validate")]
    [ProducesResponseType(typeof(ValidationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateProfilePermissionsAsync(string name, [FromBody] ValidationRequestDto request)
    {          
        _logger.LogInformation($"Validando permissões para o perfil: {name}");

        var response = await _profileService.ValidateProfilePermissionsAsync(name, request.Actions);
        
        return Ok(response);
    }
}
