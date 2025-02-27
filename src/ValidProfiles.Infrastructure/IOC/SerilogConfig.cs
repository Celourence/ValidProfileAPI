using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Net;
using ValidProfiles.Domain.Constants;

namespace ValidProfiles.Infrastructure.IOC;

public static class SerilogConfig
{
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        try
        {
            var env = builder.Environment;
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
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: Path.Combine(logPath, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            if (env.IsDevelopment())
            {
                loggerConfig
                    .MinimumLevel.Debug()
                    .WriteTo.Debug();
            }
            else
            {
                loggerConfig
                    .MinimumLevel.Information();
            }

            Log.Logger = loggerConfig.CreateLogger();

            builder.Host.UseSerilog();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring Serilog: {ex.Message}");
        }

        return builder;
    }
    
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.Request.Host.HasValue)
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                }
                
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                
                if (httpContext.Connection.RemoteIpAddress != null)
                {
                    diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress.ToString());
                }
                
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userName = httpContext.User.Identity.Name;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        diagnosticContext.Set("UserId", userName);
                    }
                }
            };
            
            options.GetLevel = (httpContext, elapsed, ex) => {
                return GetLogEventLevel(
                    ex, 
                    elapsed, 
                    httpContext.Response.StatusCode);
            };
        });

        return app;
    }
    
    private static LogEventLevel GetLogEventLevel(Exception? ex, double elapsedMs, int statusCode)
    {
        if (ex != null)
            return LogEventLevel.Error;
            
        if (elapsedMs > 5000)
            return LogEventLevel.Warning;
            
        if (statusCode > 499)
            return LogEventLevel.Error;
            
        return LogEventLevel.Information;
    }
} 