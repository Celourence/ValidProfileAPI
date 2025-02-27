using Microsoft.Extensions.Caching.Memory;
using ValidProfiles.Domain;
using ValidProfiles.Infrastructure.Cache;

namespace ValidProfiles.Tests
{
    public class ProfileCacheTests
    {
        [Fact]
        public async Task GetCachedProfile_WhenNotInCache_ShouldReturnNull()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var profileCache = new ProfileCache(memoryCache);

            // Act
            var result = await profileCache.GetAsync("Admin");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAndGetCachedProfile_ShouldReturnCachedProfile()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var profileCache = new ProfileCache(memoryCache);
            var profileParameter = new ProfileParameter
            {
                ProfileName = "Admin",
                Parameters = new Dictionary<string, string> { { "CanEdit", "true" }, { "CanDelete", "false" } }
            };

            // Act
            await profileCache.SetAsync("Admin", profileParameter);
            var result = await profileCache.GetAsync("Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.ProfileName);
            Assert.Equal("true", result.Parameters["CanEdit"]);
            Assert.Equal("false", result.Parameters["CanDelete"]);
        }
    }
} 