using ValidProfiles.Application.Services;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Infrastructure.Repositories;
using ValidProfiles.Infrastructure.IOC;
using Serilog;
using ValidProfiles.API.Middleware;
using Microsoft.Extensions.DependencyInjection;

try
{
    
    var builder = WebApplication.CreateBuilder(args);

    
    builder.ConfigureSerilog();

    
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(SwaggerConfig.Configure);
    
    
    builder.Services.AddSingleton<IProfileRepository, ProfileRepository>();
    builder.Services.AddSingleton<IProfileService, ProfileService>();

    
    var app = builder.Build();
    app.UseGlobalExceptionHandling();
    app.UseRouting();


    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValidProfiles API v1"));

    app.UseSerilogRequestLogging();

    app.MapControllers();

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