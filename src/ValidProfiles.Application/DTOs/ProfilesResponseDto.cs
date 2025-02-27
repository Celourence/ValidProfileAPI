using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ValidProfiles.Application.DTOs;

/// <summary>
/// DTO de resposta contendo a lista de perfis
/// </summary>
public class ProfilesResponseDto
{
    /// <summary>
    /// Lista de perfis
    /// </summary>
    [JsonPropertyName("profiles")]
    [SwaggerSchema("Lista de perfis")]
    public List<ProfileResponseDto> Profiles { get; set; } = new();
} 