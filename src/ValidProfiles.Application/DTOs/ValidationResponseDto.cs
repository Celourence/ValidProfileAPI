namespace ValidProfiles.Application.DTOs
{
    /// <summary>
    /// DTO para resposta da validação de permissões de um perfil
    /// </summary>
    public class ValidationResponseDto
    {
        /// <summary>
        /// Dicionário contendo as ações e seus respectivos resultados de validação
        /// </summary>
        public Dictionary<string, string> Results { get; set; } = new();
    }
} 