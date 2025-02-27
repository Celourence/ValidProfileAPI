using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Constants;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Infrastructure.BackgroundServices
{
    public class ProfileUpdateBackgroundService : BackgroundService
    {
        // Membros estáticos primeiro
        private static readonly object _lockObject = new object();
        
        // Membros de instância
        private readonly ILogger<ProfileUpdateBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval;

        public ProfileUpdateBackgroundService(
            ILogger<ProfileUpdateBackgroundService> logger,
            IServiceProvider serviceProvider,
            TimeSpan interval)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _interval = interval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(LogMessages.BackgroundService.ProfileUpdateStarted);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateProfileAsync();
                    _logger.LogInformation(LogMessages.BackgroundService.ProfileUpdateCompleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, LogMessages.BackgroundService.ProfileUpdateError);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(LogMessages.BackgroundService.ProfileUpdateStopped);
            await base.StopAsync(cancellationToken);
        }
        
        private bool ProduceRandomValue()
        {
            lock (_lockObject)
            {
                // Usando RandomNumberGenerator para geração de números aleatórios seguros
                return RandomNumberGenerator.GetInt32(100) > 50;
            }
        }

        private async Task UpdateProfileAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetService<IProfileRepository>();

            if (repository == null)
            {
                _logger.LogError(LogMessages.BackgroundService.RepositoryNotFound);
                return;
            }

            var profiles = await repository.GetProfilesAsync();

            foreach (var profile in profiles)
            {
                try
                {
                    var updatedParameters = new Dictionary<string, bool>();

                    foreach (var param in profile.Parameters)
                    {
                        updatedParameters[param.Key] = ProduceRandomValue();
                    }

                    profile.Parameters = updatedParameters;
                    await repository.UpdateProfileAsync(profile);
                    
                    _logger.LogInformation(
                        LogMessages.BackgroundService.ProfileUpdated,
                        profile.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        LogMessages.BackgroundService.UpdateError,
                        profile.Name);
                }
            }
        }
    }
}