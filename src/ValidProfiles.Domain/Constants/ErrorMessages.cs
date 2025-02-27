namespace ValidProfiles.Domain.Constants
{
    public static class ErrorMessages
    {
        public static class Profile
        {
            public const string ProfileNotFound = "Profile not found";
            public const string ProfileAlreadyExists = "A profile with this name already exists";
            public const string ParameterAlreadyExists = "This parameter already exists for the specified profile";
            public const string InvalidProfileName = "Invalid profile name";
            public const string EmptyParameterName = "Parameter name cannot be empty";
            public const string EmptyParameterValue = "Parameter value cannot be empty";
        }
        
        public static class Validation
        {
            public const string RequiredField = "The field {0} is required";
            public const string InvalidFormat = "The field {0} has an invalid format";
            public const string MaxLength = "The field {0} must have a maximum of {1} characters";
            public const string MinLength = "The field {0} must have a minimum of {1} characters";
        }
        
        public static class Authentication
        {
            public const string Unauthorized = "Unauthorized";
            public const string InvalidCredentials = "Invalid credentials";
            public const string InvalidToken = "Invalid or expired token";
        }
        
        public static class General
        {
            public const string InternalServerError = "Internal server error";
            public const string ServiceUnavailable = "Service temporarily unavailable";
            public const string BadRequest = "Bad request";
        }
    }
} 