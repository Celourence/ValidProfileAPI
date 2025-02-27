using System.ComponentModel.DataAnnotations;

namespace ValidProfiles.Application.DTOs
{
    /// <summary>
    /// DTO para validação de permissões de um perfil
    /// </summary>
    public class ValidationRequestDto
    {
        /// <summary>
        /// Lista de ações a serem validadas
        /// </summary>
        [Required]
        public List<string> Actions { get; set; } = new List<string>();
    }
} 