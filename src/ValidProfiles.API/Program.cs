using Serilog;
using ValidProfiles.API.Middleware;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Application.Services;
using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Infrastructure.Cache;
using ValidProfiles.Infrastructure.Repositories;
using ValidProfiles.Infrastructure.IOC;

var builder = WebApplication.CreateBuilder(args);

SerilogConfig.ConfigureSerilog(builder);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IProfileCache, ProfileCache>();
builder.Services.AddSingleton<IProfileCacheService, ProfileCacheService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerConfig.Configure);
builder.Services.AddSingleton<IProfileService, ProfileService>();
builder.Services.AddSingleton<IProfileRepository, ProfileRepository>();

var app = builder.Build();

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValidProfiles API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();

public partial class Program { }