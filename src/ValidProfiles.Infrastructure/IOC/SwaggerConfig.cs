using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ValidProfiles.Shared
{
namespace ValidProfiles.API
{
    public static class SwaggerConfig
    {
        public static void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "ValidProfiles API", Version = "v1" });
        }
    }
}
}
