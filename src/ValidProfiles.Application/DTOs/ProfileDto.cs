using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ValidProfiles.Application.DTOs;

public class ProfileDto
{
    [Required]
    [StringLength(50)]
    [JsonPropertyName("profileName")]
    [SwaggerSchema("Nome do perfil", Nullable = false)]
    public string Name { get; set; }

    [Required]
    [JsonPropertyName("parameters")]
    [SwaggerSchema("Parâmetros do perfil", Nullable = false)]
    public Dictionary<string, string> Parameters { get; set; }
}
