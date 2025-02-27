using System.Net;

namespace ValidProfiles.Domain.Exceptions
{
    public abstract class CustomException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorCode { get; }

        protected CustomException(string message, HttpStatusCode statusCode, string errorCode) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    public class NotFoundException : CustomException
    {
        public NotFoundException(string message = "Recurso não encontrado", string errorCode = "ERR-NF-001") 
            : base(message, HttpStatusCode.NotFound, errorCode) { }
    }

    public class BadRequestException : CustomException
    {
        public BadRequestException(string message = "Requisição inválida", string errorCode = "ERR-BR-001") 
            : base(message, HttpStatusCode.BadRequest, errorCode) { }
    }

    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message = "Acesso não autorizado", string errorCode = "ERR-AU-001") 
            : base(message, HttpStatusCode.Unauthorized, errorCode) { }
    }
    
    public class ConflictException : CustomException
    {
        public ConflictException(string message = "Conflito de dados", string errorCode = "ERR-CF-001") 
            : base(message, HttpStatusCode.Conflict, errorCode) { }
    }
} 