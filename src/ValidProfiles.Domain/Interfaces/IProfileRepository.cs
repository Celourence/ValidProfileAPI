namespace ValidProfiles.Domain.Interfaces;

public interface IProfileRepository
{
    IEnumerable<Profile> GetProfiles();
    void AddProfile(Profile profile);
}
