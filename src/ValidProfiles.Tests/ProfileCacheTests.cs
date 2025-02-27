using Microsoft.Extensions.Caching.Memory;
using ValidProfiles.Domain;
using ValidProfiles.Infrastructure.Cache;

namespace ValidProfiles.Tests
{
    public class ProfileCacheTests
    {
        [Fact]
        public async Task GetAsync_WithNonExistentKey_ShouldReturnNull()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var profileCache = new ProfileCache(memoryCache);

            // Act
            var result = await profileCache.GetAsync("NonExistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetGetAsync_WithValidData_ShouldSetAndGet()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var profileCache = new ProfileCache(memoryCache);
            var profileParameter = new ProfileParameter
            {
                ProfileName = "Admin",
                Parameters = new Dictionary<string, bool> { { "CanEdit", true }, { "CanDelete", false } }
            };

            // Act
            await profileCache.SetAsync("Admin", profileParameter);
            var result = await profileCache.GetAsync("Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.ProfileName);
            Assert.True(result.Parameters["CanEdit"]);
            Assert.False(result.Parameters["CanDelete"]);
        }
    }
} 