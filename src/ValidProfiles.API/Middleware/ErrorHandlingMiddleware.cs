using System.Net;
using System.Text.Json;
using ValidProfiles.Domain.Constants;
using ValidProfiles.Domain.Exceptions;

namespace ValidProfiles.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CustomException ex)
        {
            _logger.LogWarning(LogMessages.Middleware.DomainError, 
                ex.Message, ex.ErrorCode, ex.StatusCode);
                
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(LogMessages.Middleware.UnhandledError, ex.Message);
            
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        
        var errorResponse = new ErrorResponse
        {
            Success = false,
            Error = new ErrorDetails
            {
                Message = ErrorMessages.General.InternalServerError,
                Code = ErrorCodes.General.InternalServerError
            }
        };
        
        switch (exception)
        {
            case CustomException customException:
                response.StatusCode = (int)customException.StatusCode;
                errorResponse.Error = new ErrorDetails
                {
                    Message = customException.Message,
                    Code = customException.ErrorCode,
                    StatusCode = (int)customException.StatusCode
                };
                break;
                
            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Error = new ErrorDetails
                {
                    Message = ErrorMessages.Profile.ProfileNotFound,
                    Code = ErrorCodes.General.NotFound,
                    StatusCode = (int)HttpStatusCode.NotFound
                };
                break;
                
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = new ErrorDetails
                {
                    Message = ErrorMessages.General.InternalServerError,
                    Code = ErrorCodes.General.InternalServerError,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
                break;
        }
        
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Serialize(errorResponse, jsonOptions);
        
        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public bool Success { get; set; }
    public required ErrorDetails Error { get; set; }
}

public class ErrorDetails
{
    public required string Message { get; set; }
    public required string Code { get; set; }
    public int StatusCode { get; set; }
}