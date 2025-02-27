using ValidProfiles.Application.Services;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Infrastructure.Cache;
using ValidProfiles.Infrastructure.Repositories;
using ValidProfiles.Infrastructure.IOC;
using Serilog;
using ValidProfiles.API.Middleware;
using ValidProfiles.Infrastructure.BackgroundServices;

try
{
    // Cria o builder da aplicação
    var builder = WebApplication.CreateBuilder(args);

    // Configuração do Serilog
    builder.ConfigureSerilog();

    // Adiciona os serviços de cache
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<IProfileCache, ProfileCache>();
    builder.Services.AddSingleton<IProfileCacheService, ProfileCacheService>();

    // Adiciona os outros serviços
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(SwaggerConfig.Configure);
    builder.Services.AddSingleton<IProfileService, ProfileService>();
    builder.Services.AddSingleton<IProfileRepository, ProfileRepository>();

    // Adiciona o serviço de background para atualização periódica com intervalo de 5 minutos
    builder.Services.AddHostedService<ProfileUpdateBackgroundService>(sp => 
        new ProfileUpdateBackgroundService(
            sp.GetRequiredService<ILogger<ProfileUpdateBackgroundService>>(),
            sp,
            TimeSpan.FromMinutes(5)
        )
    );

    // Constrói a aplicação
    var app = builder.Build();

    // Adiciona o middleware de tratamento global de exceções - deve ser o primeiro para capturar todas as exceções
    app.UseGlobalExceptionHandling();

    // Configuração da pipeline de requisições HTTP
    app.UseHttpsRedirection();
    app.UseRouting();

    // Configura Swagger para todos os ambientes
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValidProfiles API v1"));

    // Configuração do Serilog para logging de requisições
    app.UseSerilogRequestLogging();

    app.MapControllers();

    // Executa a aplicação
    app.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}