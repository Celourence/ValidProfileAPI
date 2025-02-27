using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace ValidProfiles.Shared;

/// <summary>
/// Configurações do Swagger para a API
/// </summary>
public static class SwaggerConfig
{
    /// <summary>
    /// Configura o Swagger para a API
    /// </summary>
    public static void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "ValidProfiles API",
            Description = "API para gerenciamento de perfis válidos",
            Contact = new OpenApiContact
            {
                Name = "Suporte",
                Email = "suporte@validprofiles.com"
            }
        });

        // Adiciona suporte para JWT Bearer token
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Adiciona descrições XML dos endpoints, se disponível
        var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
} 