using FluentAssertions;
using Moq;
using ValidProfiles.Application;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Tests
{
    public class ProfileServiceTests
    {
        private readonly Mock<IProfileRepository> _profileRepositoryMock;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _profileRepositoryMock = new Mock<IProfileRepository>();
            _profileService = new ProfileService(_profileRepositoryMock.Object);
        }

        [Fact]
        public void GetProfiles_ReturnsAllProfiles()
        {
            // Arrange
            var profiles = new List<Profile>
            {
                new Profile { Name = "Profile1", Parameters = new Dictionary<string, string> { { "Param1", "Value1" } } },
                new Profile { Name = "Profile2", Parameters = new Dictionary<string, string> { { "Param2", "Value2" } } }
            };

            _profileRepositoryMock.Setup(repo => repo.GetProfiles()).Returns(profiles);

            // Act
            var result = _profileService.GetProfiles().ToList();

            // Assert
            result.Should().BeEquivalentTo(profiles);
        }

        [Fact]
        public void AddProfile_ValidProfile_AddsProfile()
        {
            // Arrange
            var profile = new Profile { Name = "Profile1", Parameters = new Dictionary<string, string> { { "Param1", "Value1" } } };

            // Act
            _profileService.AddProfile(profile);

            // Assert
            _profileRepositoryMock.Verify(repo => repo.AddProfile(profile), Times.Once);
        }
    }
}