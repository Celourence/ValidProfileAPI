using Microsoft.Extensions.Logging;
using Moq;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Services;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Constants;
using ValidProfiles.Domain.Exceptions;
using ValidProfiles.Domain.Interfaces;

namespace ValidProfiles.Tests
{
    public class ProfileServiceTests
    {
        private readonly Mock<IProfileRepository> _repositoryMock;
        private readonly Mock<ILogger<ProfileService>> _loggerMock;
        private readonly ProfileService _service;

        public ProfileServiceTests()
        {
            _repositoryMock = new Mock<IProfileRepository>();
            _loggerMock = new Mock<ILogger<ProfileService>>();
            _service = new ProfileService(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetProfileByNameAsync_WithValidName_ShouldReturnProfile()
        {
            // Arrange
            var profileName = "TestProfile";
            var profile = new Profile
            {
                Name = profileName,
                Parameters = new Dictionary<string, bool> 
                { 
                    { "param1", true }, 
                    { "param2", false } 
                }
            };

            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .ReturnsAsync(profile);

            // Act
            var result = await _service.GetProfileByNameAsync(profileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(profileName, result.Name);
            Assert.Equal(2, result.Parameters.Count);
            Assert.True(result.Parameters["param1"]);
            Assert.False(result.Parameters["param2"]);
            
            // Verifica que o repositório foi chamado
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
        }

        [Fact]
        public async Task GetProfileByNameAsync_WithEmptyName_ShouldThrowBadRequestException()
        {
            // Arrange
            string profileName = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.GetProfileByNameAsync(profileName));

            Assert.Equal(ErrorMessages.Profile.InvalidProfileName, exception.Message);
            
            // Verifica que o repositório não foi chamado
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetProfileByNameAsync_WithNonExistentName_ShouldThrowNotFoundException()
        {
            // Arrange
            var profileName = "NonExistentProfile";
            
            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .ReturnsAsync((Profile?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GetProfileByNameAsync(profileName));

            Assert.Equal(ErrorMessages.Profile.ProfileNotFound, exception.Message);
            
            // Verifica que o repositório foi chamado
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
        }
    }
} 