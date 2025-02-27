namespace ValidProfiles.Domain;

public class Profile
{
    public required string Name { get; set; }
    public Dictionary<string, bool> Parameters { get; set; } = new();
}
