namespace ValidProfiles.Domain.Constants
{
    public static class ErrorMessages
    {
        public static class Profile
        {
            public const string ProfileNotFound = "Perfil não encontrado";
            public const string ProfileAlreadyExists = "Um perfil com este nome já existe";
            public const string ParameterAlreadyExists = "Este parâmetro já existe para o perfil especificado";
            public const string InvalidProfileName = "Nome de perfil inválido";
            public const string EmptyParameterName = "Nome do parâmetro não pode ser vazio";
            public const string EmptyParameterValue = "Valor do parâmetro não pode ser vazio";
        }
        
        public static class Validation
        {
            public const string RequiredField = "O campo {0} é obrigatório";
            public const string InvalidFormat = "O campo {0} está em formato inválido";
            public const string MaxLength = "O campo {0} deve ter no máximo {1} caracteres";
            public const string MinLength = "O campo {0} deve ter no mínimo {1} caracteres";
        }
        
        public static class Authorization
        {
            public const string Unauthorized = "Acesso não autorizado";
            public const string InvalidToken = "Token inválido ou expirado";
            public const string InsufficientPermissions = "Permissões insuficientes para esta operação";
        }
        
        public static class General
        {
            public const string InternalServerError = "Erro interno do servidor";
            public const string ServiceUnavailable = "Serviço temporariamente indisponível";
            public const string BadRequest = "Requisição inválida";
        }
    }
} 