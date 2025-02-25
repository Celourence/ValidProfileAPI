namespace ValidProfiles.Domain;

public interface IProfileService
{
    IEnumerable<Profile> GetProfiles();
    void AddProfile(Profile profile);
}

