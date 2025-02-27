using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ValidProfiles.Infrastructure.IOC;

public static class SerilogConfig
{
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        try
        {
            var env = builder.Environment;
            var configuration = builder.Configuration;
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("ApplicationName", "ValidProfiles.API")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}")
                .WriteTo.Debug(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}");

            if (!env.IsDevelopment())
            {
                loggerConfig.WriteTo.File(
                    Path.Combine(logPath, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}",
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    retainedFileCountLimit: 30);
            }

            Log.Logger = loggerConfig.CreateLogger();

            builder.Host.UseSerilog();
            Log.Information("API ValidProfiles iniciando no ambiente {Environment}...", env.EnvironmentName);

            return builder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao configurar Serilog: {ex.Message}");
            
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
                
            Log.Error(ex, "Erro ao configurar o Serilog");
            throw;
        }
    }

    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = 
                "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var host = httpContext.Request.Host.Value;
                if (!string.IsNullOrEmpty(host))
                {
                    diagnosticContext.Set("RequestHost", host);
                }
                
                var scheme = httpContext.Request.Scheme;
                if (!string.IsNullOrEmpty(scheme))
                {
                    diagnosticContext.Set("RequestScheme", scheme);
                }
                
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (!string.IsNullOrEmpty(userAgent))
                {
                    diagnosticContext.Set("UserAgent", userAgent);
                }
                
                var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
                if (remoteIpAddress != null)
                {
                    diagnosticContext.Set("ClientIP", remoteIpAddress);
                }
                
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    var userName = httpContext.User.Identity.Name;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        diagnosticContext.Set("UserId", userName);
                    }
                }
            };
            options.GetLevel = (httpContext, elapsed, ex) => 
                ex != null ? LogEventLevel.Error :
                elapsed > 5000 ? LogEventLevel.Warning :
                httpContext.Response.StatusCode > 499 ? LogEventLevel.Error : 
                LogEventLevel.Information;
        });

        return app;
    }
} 