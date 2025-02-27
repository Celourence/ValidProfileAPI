namespace ValidProfiles.Domain;

public class Profile
{
    public required string Name { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}
