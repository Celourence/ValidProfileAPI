namespace ValidProfiles.Domain
{
    public class ProfileParameter
    {
        public required string ProfileName { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
} 