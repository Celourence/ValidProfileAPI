using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Infrastructure.BackgroundServices
{
    public class ProfileUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger<ProfileUpdateBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _updateInterval;

        public ProfileUpdateBackgroundService(
            ILogger<ProfileUpdateBackgroundService> logger,
            IServiceProvider serviceProvider,
            TimeSpan? updateInterval = null)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _updateInterval = updateInterval ?? TimeSpan.FromMinutes(5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de atualização periódica iniciado. Intervalo definido para {Interval:F4} minutos.", _updateInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Iniciando atualização dos parâmetros de perfis...");
                    await UpdateProfileParameters();
                    _logger.LogInformation("Atualização dos parâmetros concluída. Próxima atualização em {Interval:F4} minutos.", _updateInterval.TotalMinutes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar os parâmetros dos perfis.");
                }

                // Aguarda o intervalo especificado antes da próxima atualização
                await Task.Delay(_updateInterval, stoppingToken);
            }
        }

        private async Task UpdateProfileParameters()
        {
            // Usamos um escopo para obter os serviços necessários
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IProfileRepository>();
                var cache = scope.ServiceProvider.GetRequiredService<IProfileCache>();

                // Obter todos os perfis do repositório
                var profiles = await repository.GetProfilesAsync();
                var profilesList = profiles.ToList();

                if (profilesList.Count == 0)
                {
                    _logger.LogInformation("Nenhum perfil encontrado para atualizar.");
                    return;
                }

                // Atualizar cada perfil
                foreach (var profile in profilesList)
                {
                    // Alternar os valores dos parâmetros (true para false e vice-versa)
                    var updatedParameters = new Dictionary<string, bool>();
                    foreach (var param in profile.Parameters)
                    {
                        updatedParameters[param.Key] = !param.Value;
                    }

                    // Atualizar o perfil com os novos parâmetros
                    profile.Parameters = updatedParameters;
                    await repository.UpdateProfileAsync(profile);

                    // Atualizar o cache também
                    var profileParameter = new ProfileParameter
                    {
                        ProfileName = profile.Name,
                        Parameters = updatedParameters
                    };
                    await cache.SetAsync(profile.Name, profileParameter);

                    _logger.LogInformation("Perfil {ProfileName} atualizado. Parâmetros alternados.", profile.Name);
                }

                _logger.LogInformation("Total de {ProfileCount} perfis atualizados com sucesso.", profilesList.Count);
            }
        }
    }
} 