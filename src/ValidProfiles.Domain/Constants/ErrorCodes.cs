namespace ValidProfiles.Domain.Constants
{
    /// <summary>
    /// Códigos de erro padronizados para a aplicação
    /// </summary>
    public static class ErrorCodes
    {
        public static class Profile
        {
            public const string NotFound = "ERR-NF-001";
            public const string AlreadyExists = "ERR-CF-001";
            public const string InvalidFormat = "ERR-BR-001";
            public const string InvalidParameter = "ERR-BR-002";
        }
        
        public static class General
        {
            public const string NotFound = "ERR-NF-002";
            public const string InternalServerError = "ERR-IS-001";
            public const string BadRequest = "ERR-BR-003";
            public const string Unauthorized = "ERR-AU-001";
        }
    }
} 