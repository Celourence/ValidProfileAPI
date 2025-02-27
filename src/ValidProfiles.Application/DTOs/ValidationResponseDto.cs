using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValidProfiles.Application.DTOs
{
    /// <summary>
    /// Representa a resposta de validação de permissões de um perfil
    /// </summary>
    public class ValidationResponseDto
    {
        /// <summary>
        /// Nome do perfil validado
        /// </summary>
        [JsonPropertyName("profileName")]
        public required string ProfileName { get; set; }
        
        /// <summary>
        /// Resultados das validações de permissões
        /// Chave: Nome da ação
        /// Valor: Resultado da validação (Allowed, Denied, Undefined)
        /// </summary>
        [JsonPropertyName("results")]
        public Dictionary<string, string> Results { get; set; } = new Dictionary<string, string>();
    }
} 