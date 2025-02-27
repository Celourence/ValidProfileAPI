namespace ValidProfiles.API.Middleware;

/// <summary>
/// Extensões para configurar middlewares
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adiciona o middleware global de tratamento de exceções
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) => 
        app.UseMiddleware<ErrorHandlingMiddleware>();
} 