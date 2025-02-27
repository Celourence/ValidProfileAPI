using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ValidProfiles.Application.Interfaces;

namespace ValidProfiles.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Serviço de atualização periódica de perfis em segundo plano
    /// </summary>
    public class ProfileUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger<ProfileUpdateBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval;

        /// <summary>
        /// Construtor do serviço de atualização periódica
        /// </summary>
        /// <param name="logger">Logger para registro de atividades</param>
        /// <param name="serviceProvider">Provedor de serviços para resolução de dependências</param>
        /// <param name="interval">Intervalo entre atualizações</param>
        public ProfileUpdateBackgroundService(
            ILogger<ProfileUpdateBackgroundService> logger,
            IServiceProvider serviceProvider,
            TimeSpan interval)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _interval = interval;

            _logger.LogInformation("Periodic update service started. Interval set to {Interval} minutes.", 
                interval.TotalMinutes);
        }

        /// <summary>
        /// Executa a tarefa em segundo plano
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateProfilesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during periodic profile update");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        /// <summary>
        /// Atualiza os perfis utilizando o serviço apropriado
        /// </summary>
        private async Task UpdateProfilesAsync()
        {
            _logger.LogInformation("Starting profile parameters update...");
            
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();

                    var profiles = await profileService.GetProfilesAsync();
                    var profilesList = profiles.Profiles.ToList();

                    if (profilesList.Count == 0)
                    {
                        _logger.LogInformation("No profiles found to update.");
                    }
                    else
                    {
                        _logger.LogInformation("Found {Count} profiles to update.", profilesList.Count);
                        // Aqui poderia implementar lógica adicional de atualização se necessário
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update profiles");
                    throw;
                }
            }
            
            _logger.LogInformation("Parameters update completed. Next update in {Interval} minutes.", 
                _interval.TotalMinutes);
        }
    }
} 