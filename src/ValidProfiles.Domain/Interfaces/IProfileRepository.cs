namespace ValidProfiles.Domain.Interfaces;

public interface IProfileRepository
{
    Task<IEnumerable<Profile>> GetProfilesAsync();
    Task AddProfileAsync(Profile profile);
}
