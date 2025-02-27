namespace ValidProfiles.Domain.Constants
{
    public static class LogMessages
    {
        public static class BackgroundService
        {
            public const string ServiceStarted = "Periodic update service started with interval of {0} minutes";
            public const string UpdateError = "Error updating profile {0}";
            public const string RepositoryNotResolved = "Could not resolve IProfileRepository. Updates will not be performed";
            public const string RepositoryNotFound = "Profile repository service not found. Updates will not be performed";
            public const string UpdatingProfiles = "Updating {0} profiles";
            public const string NoProfilesToUpdate = "No profiles to update";
            public const string GeneratingRandomValues = "Generating random values for profile {0}";
            public const string ParameterChanged = "Profile {0}: Parameter '{1}' changed from {2} to {3}";
            public const string ProfileUpdated = "Profile {0} successfully updated";
            public const string UpdateCompleted = "Update completed at {0:HH:mm:ss.fff}. Next update in {1} minutes";
            public const string ProfileUpdateStarted = "Profile update service started";
            public const string ProfileUpdateCompleted = "Profile update iteration completed";
            public const string ProfileUpdateError = "Error during profile update execution";
            public const string ProfileUpdateStopped = "Profile update service stopped";
        }
        
        public static class Application
        {
            public const string ApplicationStarting = "ValidProfiles API starting in {0} environment...";
            public const string ApplicationStartFailed = "Application failed to start";
            public const string ConfigurationError = "Error configuring {0}: {1}";
        }
        
        public static class Api
        {
            public const string RequestTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            public const string RequestReceived = "Request received: {0} {1}";
            public const string RequestCompleted = "Request completed: {0} {1} - {2} in {3}ms";
            public const string RequestError = "Error processing request: {0} {1}";
        }

        public static class Middleware
        {
            public const string DomainError = "Domain error: {Message}, Code: {ErrorCode}, Status: {StatusCode}";
            public const string UnhandledError = "Unhandled error: {Message}";
            public const string HandlingException = "Handling exception of type {ExceptionType}";
            public const string ResponseSent = "Error response sent with status code {StatusCode}";
        }

        public static class ProfileService
        {
            public const string ProfileNotFound = "Profile '{0}' not found";
            public const string StartingProfileCreation = "Starting profile creation: {0}";
            public const string ProfileCreated = "Profile created successfully: {0}";
            public const string StartingProfileUpdate = "Starting profile update: {0}";
            public const string ProfileUpdated = "Profile updated successfully: {0}";
            public const string StartingProfileDeletion = "Starting profile deletion: {0}";
            public const string ProfileDeleted = "Profile deleted successfully: {0}";
            public const string ValidatingPermissions = "Validating permissions for profile: {0}";
            public const string PermissionValidationResult = "Permission validation result for profile {0}: {1}";
            public const string UpdatingParameter = "Updating parameter '{0}' for profile '{1}'";
            public const string ParameterUpdated = "Parameter '{0}' updated for profile '{1}'";
            public const string AttemptingToRemoveParameter = "Attempting to remove parameter '{0}' from profile '{1}'";
            public const string ParameterRemoved = "Parameter '{0}' removed from profile '{1}'";
        }
    }
} 