using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ValidProfiles.Application.DTOs;

/// <summary>
/// DTO para criação de perfil
/// </summary>
public class ProfileDto
{
    /// <summary>
    /// Nome do perfil
    /// </summary>
    [Required(ErrorMessage = "Profile name is required")]
    [StringLength(50, ErrorMessage = "Profile name must have a maximum of 50 characters")]
    [JsonPropertyName("profileName")]
    [SwaggerSchema("Nome do perfil", Nullable = false)]
    public required string Name { get; set; }

    /// <summary>
    /// Parâmetros do perfil
    /// </summary>
    /// <remarks>
    /// Deve conter pelo menos um parâmetro.
    /// Todos os valores são do tipo booleano.
    /// </remarks>
    [JsonPropertyName("parameters")]
    [SwaggerSchema("Parâmetros do perfil (valores booleanos)")]
    [Required(ErrorMessage = "At least one parameter is required")]
    public Dictionary<string, bool> Parameters { get; set; } = new();
}
