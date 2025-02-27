using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ValidProfiles.Application.DTOs;

/// <summary>
/// DTO de resposta para perfil
/// </summary>
public class ProfileResponseDto
{
    [JsonPropertyName("profileName")]
    [SwaggerSchema("Nome do perfil")]
    public required string Name { get; set; }

    [JsonPropertyName("parameters")]
    [SwaggerSchema("Parâmetros do perfil")]
    public Dictionary<string, bool> Parameters { get; set; } = new();
} 