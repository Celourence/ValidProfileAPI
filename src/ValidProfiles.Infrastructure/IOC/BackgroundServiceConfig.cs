using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ValidProfiles.Infrastructure.BackgroundServices;

namespace ValidProfiles.Infrastructure.IOC
{
    public static class BackgroundServiceConfig
    {

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService(sp => new ProfileUpdateBackgroundService(
                sp.GetRequiredService<ILogger<ProfileUpdateBackgroundService>>(),
                sp,
                TimeSpan.FromMinutes(5)
            ));
            
            return services;
        }
    }
} 