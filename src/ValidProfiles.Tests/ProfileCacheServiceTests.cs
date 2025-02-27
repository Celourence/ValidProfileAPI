using Moq;
using ValidProfiles.Application.Services;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Application.DTOs;

namespace ValidProfiles.Tests
{
    public class ProfileCacheServiceTests
    {
        private readonly Mock<IProfileRepository> _repositoryMock;
        private readonly Mock<IProfileCache> _cacheMock;
        private readonly ProfileCacheService _service;
        private readonly Mock<IProfileService> _profileServiceMock;

        public ProfileCacheServiceTests()
        {
            _repositoryMock = new Mock<IProfileRepository>();
            _cacheMock = new Mock<IProfileCache>();
            _profileServiceMock = new Mock<IProfileService>();
            var loggerMock = new Mock<ILogger<ProfileCacheService>>();
            _service = new ProfileCacheService(
                _cacheMock.Object, 
                _repositoryMock.Object, 
                loggerMock.Object,
                _profileServiceMock.Object);
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

            // Configurar o repositório para retornar o perfil diretamente
            _repositoryMock.Setup(r => r.GetProfileByNameAsync("Admin"))
                .ReturnsAsync(profiles.First());

            // Act
            var result = await _service.GetProfileParameterAsync("Admin");

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(r => r.GetProfileByNameAsync("Admin"), Times.Once);
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
            var result = await _service.AddProfileParameterAsync(profileParameter);

            // Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(r => r.AddProfileAsync(It.IsAny<Profile>()), Times.Once);
            _cacheMock.Verify(c => c.SetAsync("Admin", It.IsAny<ProfileParameter>()), Times.Once);
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithCachedProfile_ShouldReturnFromCache()
        {
            // Arrange
            var profileName = "TestProfile";
            var actions = new List<string> { "CanEdit", "CanDelete", "NonExistentAction" };
            
            var cachedProfile = new ProfileParameter
            {
                ProfileName = profileName,
                Parameters = new Dictionary<string, bool>
                {
                    { "CanEdit", true },
                    { "CanDelete", false }
                }
            };
            
            _cacheMock.Setup(cache => cache.GetAsync(profileName))
                .ReturnsAsync(cachedProfile);
            
            // Act
            var result = await _service.ValidateProfilePermissionsAsync(profileName, actions);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Results.Count);
            Assert.Equal("Allowed", result.Results["CanEdit"]);
            Assert.Equal("Denied", result.Results["CanDelete"]);
            Assert.Equal("Undefined", result.Results["NonExistentAction"]);
            
            // Verificar que o ProfileService não foi chamado
            _profileServiceMock.Verify(service => 
                service.ValidateProfilePermissionsAsync(It.IsAny<string>(), It.IsAny<List<string>>()), 
                Times.Never);
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithNonCachedProfile_ShouldCallProfileService()
        {
            // Arrange
            var profileName = "TestProfile";
            var actions = new List<string> { "CanEdit", "CanDelete" };
            
            // Configurar o cache para retornar nulo
            _cacheMock.Setup(cache => cache.GetAsync(profileName))
                .ReturnsAsync((ProfileParameter?)null);
            
            // Configurar o ProfileService para retornar uma resposta
            var expectedResponse = new ValidationResponseDto
            {
                ProfileName = "TestProfile",
                Results = new Dictionary<string, string>
                {
                    { "CanEdit", "Allowed" },
                    { "CanDelete", "Denied" }
                }
            };
            
            _profileServiceMock.Setup(service => 
                service.ValidateProfilePermissionsAsync(profileName, actions))
                .ReturnsAsync(expectedResponse);
            
            // Act
            var result = await _service.ValidateProfilePermissionsAsync(profileName, actions);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Results.Count, result.Results.Count);
            Assert.Equal("Allowed", result.Results["CanEdit"]);
            Assert.Equal("Denied", result.Results["CanDelete"]);
            
            // Verificar que o ProfileService foi chamado
            _profileServiceMock.Verify(service => 
                service.ValidateProfilePermissionsAsync(profileName, actions), 
                Times.Once);
        }
    }
} 