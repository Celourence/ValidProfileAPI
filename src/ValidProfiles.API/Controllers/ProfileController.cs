using Microsoft.AspNetCore.Mvc;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Domain;

namespace ValidProfiles.API.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IProfileService profileService, ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetProfiles()
    {
        _logger.LogInformation("Obtendo todos os perfis");
        return Ok(_profileService.GetProfiles());
    }

    [HttpPost]
    public IActionResult AddProfile([FromBody] ProfileDto profile)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var domainProfile = new Profile
        {
            Name = profile.Name,
        };
        _profileService.AddProfile(domainProfile);
        return CreatedAtAction(nameof(GetProfiles), new { domainProfile.Name }, domainProfile);
    }
}