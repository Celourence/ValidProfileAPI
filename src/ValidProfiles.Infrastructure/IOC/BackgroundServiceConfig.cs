using Microsoft.Extensions.DependencyInjection;
using ValidProfiles.Infrastructure.BackgroundServices;

namespace ValidProfiles.Infrastructure.IOC
{
    public static class BackgroundServiceConfig
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<ProfileUpdateBackgroundService>();
            
            return services;
        }
    }
} 