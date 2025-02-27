using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace ValidProfiles.Infrastructure.IOC;

/// <summary>
/// Configuração do Serilog para a aplicação
/// </summary>
public static class SerilogConfig
{
    /// <summary>
    /// Configura o Serilog de acordo com o ambiente
    /// </summary>
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        try
        {
            var env = builder.Environment;
            var configuration = builder.Configuration;
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            
            // Garante que o diretório de logs existe
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
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            // Adiciona log em arquivo se for ambiente de produção ou homologação
            if (!env.IsDevelopment())
            {
                loggerConfig.WriteTo.File(
                    Path.Combine(logPath, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB por arquivo
                    retainedFileCountLimit: 30); // Mantém arquivos de 30 dias
            }

            Log.Logger = loggerConfig.CreateLogger();

            builder.Host.UseSerilog();
            Log.Information("API ValidProfiles iniciando no ambiente {Environment}...", env.EnvironmentName);

            return builder;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao configurar Serilog: {ex.Message}");
            
            // Configura um logger mínimo para evitar que a aplicação falhe
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
                
            Log.Error(ex, "Erro ao configurar o Serilog");
            throw;
        }
    }

    /// <summary>
    /// Configura o middleware de logging de requisições HTTP
    /// </summary>
    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = 
                "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // Correção para evitar valor nulo - verificando se o Host.Value existe
                var host = httpContext.Request.Host.Value;
                if (!string.IsNullOrEmpty(host))
                {
                    diagnosticContext.Set("RequestHost", host);
                }
                
                // Correção para evitar valor nulo - verificando se o Scheme existe
                var scheme = httpContext.Request.Scheme;
                if (!string.IsNullOrEmpty(scheme))
                {
                    diagnosticContext.Set("RequestScheme", scheme);
                }
                
                // Correção para evitar valor nulo - verificando se o UserAgent existe
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (!string.IsNullOrEmpty(userAgent))
                {
                    diagnosticContext.Set("UserAgent", userAgent);
                }
                
                // Correção para evitar valor nulo - verificando se o RemoteIpAddress existe
                var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
                if (remoteIpAddress != null)
                {
                    diagnosticContext.Set("ClientIP", remoteIpAddress);
                }
                
                // Adiciona informações sobre o usuário autenticado, se houver
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    // Correção para evitar valor nulo - verificando se o Name não é nulo
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