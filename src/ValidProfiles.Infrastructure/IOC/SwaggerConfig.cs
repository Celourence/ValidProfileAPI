using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ValidProfiles.Infrastructure.IOC;

/// <summary>
/// Configurações do Swagger para a API
/// </summary>
public static class SwaggerConfig
{
    public static void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "ValidProfiles API",
            Description = "API para gerenciamento de perfis válidos",
        });

        var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
} 