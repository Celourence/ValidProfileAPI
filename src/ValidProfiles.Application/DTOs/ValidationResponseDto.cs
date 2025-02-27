using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValidProfiles.Application.DTOs
{
    /// <summary>
    /// Representa a resposta de validação de permissões de um perfil
    /// </summary>
    public class ValidationResponseDto
    {
        [JsonPropertyName("profileName")]
        public required string ProfileName { get; set; }
        
        [JsonPropertyName("results")]
        public Dictionary<string, string> Results { get; set; } = new Dictionary<string, string>();
    }
} 