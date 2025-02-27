using System.ComponentModel.DataAnnotations;

namespace ValidProfiles.Application.DTOs
{
    /// <summary>
    /// DTO para atualização de perfil
    /// </summary>
    public class ProfileUpdateDto
    {
        /// <summary>
        /// Parâmetros do perfil (nome do parâmetro e valor)
        /// </summary>
        [Required(ErrorMessage = "Os parâmetros são obrigatórios")]
        public Dictionary<string, bool> Parameters { get; set; } = new();
    }
} 