using System.ComponentModel.DataAnnotations;

namespace ValidProfiles.Application.DTOs
{
    /// <summary>
    /// DTO para atualização de perfil
    /// </summary>
    public class ProfileUpdateDto
    {
        [Required(ErrorMessage = "Parameters are required")]
        public Dictionary<string, bool> Parameters { get; set; } = new();
    }
} 