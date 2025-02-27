using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ValidProfiles.Application.DTOs;

/// <summary>
/// DTO para criação de perfil
/// </summary>
public class ProfileDto
{
    [Required(ErrorMessage = "Profile name is required")]
    [StringLength(50, ErrorMessage = "Profile name must have a maximum of 50 characters")]
    [JsonPropertyName("profileName")]
    [SwaggerSchema("Nome do perfil", Nullable = false)]
    public required string Name { get; set; }

    [Required(ErrorMessage = "At least one parameter is required")]
    [JsonPropertyName("parameters")]
    [SwaggerSchema("Parâmetros do perfil (valores booleanos)")]
    public Dictionary<string, bool> Parameters { get; set; } = new();
}
