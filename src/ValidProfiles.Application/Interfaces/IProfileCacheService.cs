using System.Collections.Generic;
using System.Threading.Tasks;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Domain;

namespace ValidProfiles.Application.Interfaces
{
    /// <summary>
    /// Interface para o serviço de cache de perfis
    /// </summary>
    public interface IProfileCacheService
    {
        /// <summary>
        /// Obtém um perfil do cache pelo nome
        /// </summary>
        /// <param name="profileName">Nome do perfil</param>
        /// <returns>O perfil encontrado ou null se não existir</returns>
        Task<ProfileResponseDto> GetProfileAsync(string profileName);

        /// <summary>
        /// Adiciona ou atualiza um perfil no cache
        /// </summary>
        /// <param name="profile">Perfil a ser adicionado/atualizado</param>
        Task SetProfileAsync(ProfileResponseDto profile);

        /// <summary>
        /// Remove um perfil do cache
        /// </summary>
        /// <param name="profileName">Nome do perfil a ser removido</param>
        Task RemoveProfileAsync(string profileName);

        /// <summary>
        /// Atualiza o cache de perfis com os dados mais recentes do repositório
        /// </summary>
        Task RefreshCacheAsync();

        /// <summary>
        /// Obtém parâmetros de um perfil do cache pelo nome
        /// </summary>
        /// <param name="profileName">Nome do perfil</param>
        /// <returns>Parâmetros do perfil ou null se não existir</returns>
        Task<ProfileParameter> GetProfileParameterAsync(string profileName);

        /// <summary>
        /// Adiciona ou atualiza parâmetros de um perfil no cache
        /// </summary>
        /// <param name="profile">Parâmetros do perfil a serem adicionados/atualizados</param>
        /// <returns>Os parâmetros do perfil adicionados/atualizados</returns>
        Task<ProfileParameter> SetProfileParameterAsync(ProfileParameter profile);

        /// <summary>
        /// Adiciona novos parâmetros de perfil ao cache
        /// </summary>
        /// <param name="profileParameter">Parâmetros do perfil a serem adicionados</param>
        /// <returns>Os parâmetros do perfil adicionados ou null se falhar</returns>
        Task<ProfileParameter> AddProfileParameterAsync(ProfileParameter profileParameter);

        /// <summary>
        /// Remove parâmetros de um perfil do cache
        /// </summary>
        /// <param name="profileName">Nome do perfil</param>
        /// <returns>True se removido com sucesso, False caso contrário</returns>
        Task<bool> RemoveProfileParameterAsync(string profileName);

        /// <summary>
        /// Limpa todo o cache de perfis
        /// </summary>
        Task ClearCacheAsync();

        /// <summary>
        /// Valida permissões de um perfil para uma lista de ações
        /// </summary>
        /// <param name="name">Nome do perfil</param>
        /// <param name="actions">Lista de ações a serem validadas</param>
        /// <returns>Resultados da validação</returns>
        Task<ValidationResponseDto> ValidateProfilePermissionsAsync(string name, List<string> actions);
    }
} 