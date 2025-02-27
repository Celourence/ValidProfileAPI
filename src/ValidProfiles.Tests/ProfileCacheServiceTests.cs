using Moq;
using ValidProfiles.Application.Services;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Tests
{
    public class ProfileCacheServiceTests
    {
        private readonly Mock<IProfileRepository> _repositoryMock;
        private readonly Mock<IProfileCache> _cacheMock;
        private readonly IProfileCacheService _service;

        public ProfileCacheServiceTests()
        {
            _repositoryMock = new Mock<IProfileRepository>();
            _cacheMock = new Mock<IProfileCache>();
            _service = new ProfileCacheService(_cacheMock.Object, _repositoryMock.Object);
        }

        [Fact]
        public async Task GetProfileParameter_WhenNotInCache_ShouldReturnFromRepository()
        {
            // Arrange
            var profiles = new List<Profile>
            {
                new Profile
                {
                    Name = "Admin",
                    Parameters = new Dictionary<string, bool>
                    {
                        { "CanView", true },
                        { "CanEdit", true }
                    }
                }
            };

            _repositoryMock.Setup(r => r.GetProfilesAsync())
                .Returns(Task.FromResult((IEnumerable<Profile>)profiles));

            _cacheMock.Setup(c => c.GetAsync("Admin"))
                .Returns(Task.FromResult<ProfileParameter?>(null));

            // Act
            var result = await _service.GetProfileParameterAsync("Admin");

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(r => r.GetProfilesAsync(), Times.Once);
            _cacheMock.Verify(c => c.SetAsync("Admin", It.IsAny<ProfileParameter>()), Times.Once);
        }

        [Fact]
        public async Task GetProfileParameter_WhenInCache_ShouldReturnFromCache()
        {
            // Arrange
            var profileParameter = new ProfileParameter
            {
                ProfileName = "User",
                Parameters = new Dictionary<string, bool>
                {
                    { "CanView", true },
                    { "CanEdit", false }
                }
            };

            _cacheMock.Setup(c => c.GetAsync("User"))
                .Returns(Task.FromResult<ProfileParameter?>(profileParameter));

            // Configura o repositório para retornar uma lista vazia para garantir que o cache foi usado
            _repositoryMock.Setup(r => r.GetProfilesAsync())
                .Returns(Task.FromResult<IEnumerable<Profile>>(new List<Profile>()));

            // Act
            var result = await _service.GetProfileParameterAsync("User");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("User", result.ProfileName);
            Assert.Equal(2, result.Parameters.Count);
            Assert.True(result.Parameters["CanView"]);
            Assert.False(result.Parameters["CanEdit"]);
            _repositoryMock.Verify(r => r.GetProfilesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllProfileParameters_ShouldReturnFromCache()
        {
            // Arrange
            var profiles = new Dictionary<string, ProfileParameter>
            {
                {
                    "Admin", new ProfileParameter
                    {
                        ProfileName = "Admin",
                        Parameters = new Dictionary<string, bool>
                        {
                            { "CanView", true },
                            { "CanEdit", true }
                        }
                    }
                },
                {
                    "User", new ProfileParameter
                    {
                        ProfileName = "User",
                        Parameters = new Dictionary<string, bool>
                        {
                            { "CanView", true },
                            { "CanEdit", false }
                        }
                    }
                }
            };

            _cacheMock.Setup(c => c.GetAllAsync())
                .Returns(Task.FromResult(profiles));

            // Act
            var result = await _service.GetAllProfileParametersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result["Admin"].Parameters["CanView"]);
            Assert.True(result["Admin"].Parameters["CanEdit"]);
            Assert.True(result["User"].Parameters["CanView"]);
            Assert.False(result["User"].Parameters["CanEdit"]);
        }

        [Fact]
        public async Task AddProfileParameter_ShouldAddToRepositoryAndCache()
        {
            // Arrange
            var profileParameter = new ProfileParameter
            {
                ProfileName = "Admin",
                Parameters = new Dictionary<string, bool>
                {
                    { "CanView", true }
                }
            };

            var savedProfile = new Profile
            {
                Name = "Admin",
                Parameters = new Dictionary<string, bool>
                {
                    { "CanView", true }
                }
            };

            var existingCache = new Dictionary<string, ProfileParameter>();

            _repositoryMock.Setup(r => r.AddProfileAsync(It.IsAny<Profile>()))
                .Returns(Task.FromResult(savedProfile));

            _cacheMock.Setup(c => c.GetAllAsync())
                .Returns(Task.FromResult(existingCache));

            // Act
            await _service.AddProfileParameterAsync(profileParameter);

            // Assert
            _repositoryMock.Verify(r => r.AddProfileAsync(It.IsAny<Profile>()), Times.Once);
            _cacheMock.Verify(c => c.SetAsync("Admin", It.IsAny<ProfileParameter>()), Times.Once);
        }
    }
} 