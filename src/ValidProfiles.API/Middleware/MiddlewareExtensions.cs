namespace ValidProfiles.API.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) => 
        app.UseMiddleware<ErrorHandlingMiddleware>();
} 