
using Serilog;
using ValidProfiles.Application;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Infrastructure.Repositories;
using ValidProfiles.Shared.ValidProfiles.API;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerConfig.Configure);
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();

var app = builder.Build();

// Configuração de Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValidProfiles API v1"));
}

app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();