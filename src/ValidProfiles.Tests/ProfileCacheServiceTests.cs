using Microsoft.Extensions.Caching.Memory;
using Moq;
using ValidProfiles.Application.Services;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Infrastructure.Cache;

namespace ValidProfiles.Tests
{
    public class ProfileCacheServiceTests
    {
        private readonly Mock<IProfileRepository> _repositoryMock;
        private readonly IProfileCache _cache;
        private readonly IProfileCacheService _service;

        public ProfileCacheServiceTests()
        {
            _repositoryMock = new Mock<IProfileRepository>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            _cache = new ProfileCache(memoryCache);
            _service = new ProfileCacheService(_cache, _repositoryMock.Object);
        }

        [Fact]
        public async Task GetProfileParameter_WhenNotInCache_ShouldReturnFromRepository()
        {
            // Arrange
            var profiles = new List<Profile>
            {
                new() { Name = "Admin", Parameters = new Dictionary<string, string> { { "CanEdit", "true" } } }
            };
            
            _repositoryMock.Setup(r => r.GetProfilesAsync())
                .ReturnsAsync(profiles);

            // Act
            var result = await _service.GetProfileParameterAsync("Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.ProfileName);
            Assert.Equal("true", result.Parameters["CanEdit"]);

            // Verifica que foi armazenado em cache
            var cachedResult = await _cache.GetAsync("Admin");
            Assert.NotNull(cachedResult);
            Assert.Equal("Admin", cachedResult.ProfileName);
        }

        [Fact]
        public async Task GetProfileParameter_WhenInCache_ShouldReturnFromCache()
        {
            // Arrange
            var profileParameter = new ProfileParameter
            {
                ProfileName = "User",
                Parameters = new Dictionary<string, string> { { "CanView", "true" }, { "CanEdit", "false" } }
            };

            await _cache.SetAsync("User", profileParameter);

            // Configura o repositório para retornar uma lista vazia para garantir que o cache foi usado
            _repositoryMock.Setup(r => r.GetProfilesAsync())
                .ReturnsAsync(new List<Profile>());

            // Act
            var result = await _service.GetProfileParameterAsync("User");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("User", result.ProfileName);
            Assert.Equal("true", result.Parameters["CanView"]);
            Assert.Equal("false", result.Parameters["CanEdit"]);

            // Verifica que o repositório não foi chamado
            _repositoryMock.Verify(r => r.GetProfilesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllProfileParameters_WhenInCache_ShouldReturnFromCache()
        {
            // Arrange
            var profiles = new Dictionary<string, ProfileParameter>
            {
                ["Admin"] = new() { ProfileName = "Admin", Parameters = new Dictionary<string, string> { { "CanView", "true" } } },
                ["User"] = new() { ProfileName = "User", Parameters = new Dictionary<string, string> { { "CanView", "true" } } }
            };

            await _cache.SetAllAsync(profiles);

            // Act
            var result = await _service.GetAllProfileParametersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("Admin", result.Keys);
            Assert.Contains("User", result.Keys);
            
            // Verifica que o repositório não foi chamado
            _repositoryMock.Verify(r => r.GetProfilesAsync(), Times.Never);
        }
        
        [Fact]
        public async Task AddProfileParameter_ShouldAddToRepositoryAndCache()
        {
            // Arrange
            var profileParameter = new ProfileParameter
            {
                ProfileName = "NewUser",
                Parameters = new Dictionary<string, string> { { "CanView", "true" } }
            };
            
            _repositoryMock.Setup(r => r.AddProfileAsync(It.IsAny<Profile>()))
                .Returns(Task.CompletedTask);
                
            // Act
            await _service.AddProfileParameterAsync(profileParameter);
            
            // Assert
            _repositoryMock.Verify(r => r.AddProfileAsync(It.Is<Profile>(p => 
                p.Name == "NewUser" && 
                p.Parameters.ContainsKey("CanView"))), 
                Times.Once);
                
            var cachedProfile = await _cache.GetAsync("NewUser");
            Assert.NotNull(cachedProfile);
            Assert.Equal("NewUser", cachedProfile.ProfileName);
            Assert.Equal("true", cachedProfile.Parameters["CanView"]);
        }
    }
} 