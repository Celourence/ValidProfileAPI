namespace ValidProfiles.Domain.Interfaces;

public interface IProfileRepository
{
    Task<IEnumerable<Profile>> GetProfilesAsync();
    Task<Profile?> GetProfileByNameAsync(string name);
    Task AddProfileAsync(Profile profile);
}
