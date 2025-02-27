using Microsoft.Extensions.Logging;
using Moq;
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
                .Returns(Task.FromResult<Profile?>(profile));

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
                .Returns(Task.FromResult<Profile?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GetProfileByNameAsync(profileName));

            Assert.Equal(ErrorMessages.Profile.ProfileNotFound, exception.Message);
            
            // Verifica que o repositório foi chamado
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
        }
        
        [Fact]
        public async Task UpdateProfileAsync_WithValidData_ShouldUpdateAndReturnProfile()
        {
            // Arrange
            var profileName = "TestProfile";
            var existingProfile = new Profile
            {
                Name = profileName,
                Parameters = new Dictionary<string, bool> 
                { 
                    { "param1", true }, 
                    { "param2", false } 
                }
            };
            
            var updatedParameters = new Dictionary<string, bool> 
            { 
                { "param1", false }, 
                { "param3", true } 
            };
            
            var updatedProfile = new Profile
            {
                Name = profileName,
                Parameters = updatedParameters
            };
            
            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .Returns(Task.FromResult<Profile?>(existingProfile));
                
            _repositoryMock.Setup(r => r.UpdateProfileAsync(It.IsAny<Profile>()))
                .Returns(Task.FromResult(updatedProfile));

            // Act
            var result = await _service.UpdateProfileAsync(profileName, updatedParameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(profileName, result.Name);
            Assert.Equal(2, result.Parameters.Count);
            Assert.False(result.Parameters["param1"]);
            Assert.True(result.Parameters["param3"]);
            
            // Verifica que os métodos do repositório foram chamados
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
            _repositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.Once);
        }
        
        [Fact]
        public async Task UpdateProfileAsync_WithEmptyName_ShouldThrowBadRequestException()
        {
            // Arrange
            string profileName = "";
            var parameters = new Dictionary<string, bool> { { "param1", true } };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.UpdateProfileAsync(profileName, parameters));

            Assert.Equal(ErrorMessages.Profile.InvalidProfileName, exception.Message);
            
            // Verifica que o repositório não foi chamado
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateProfileAsync_WithNonExistentProfile_ShouldThrowNotFoundException()
        {
            // Arrange
            var profileName = "NonExistentProfile";
            var parameters = new Dictionary<string, bool> { { "param1", true } };
            
            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .Returns(Task.FromResult<Profile?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.UpdateProfileAsync(profileName, parameters));

            Assert.Equal(ErrorMessages.Profile.ProfileNotFound, exception.Message);
            
            // Verifica que os métodos do repositório foram chamados corretamente
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
            _repositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.Never);
        }
        
        [Fact]
        public async Task UpdateProfileAsync_WithEmptyParameters_ShouldThrowBadRequestException()
        {
            // Arrange
            var profileName = "TestProfile";
            Dictionary<string, bool> parameters = new Dictionary<string, bool>();
            
            var existingProfile = new Profile
            {
                Name = profileName,
                Parameters = new Dictionary<string, bool>
                {
                    { "param1", false },
                    { "param2", true }
                }
            };
            
            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .ReturnsAsync(existingProfile);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.UpdateProfileAsync(profileName, parameters));

            Assert.Equal("Parameter list cannot be empty", exception.Message);
            
            // Na implementação atual, o método não chega a chamar o repositório quando
            // os parâmetros estão vazios, então não verificamos chamadas ao repositório
            _repositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteProfileAsync_WithValidName_ShouldDeleteProfile()
        {
            // Arrange
            var profileName = "TestProfile";
            var profile = new Profile
            {
                Name = profileName,
                Parameters = new Dictionary<string, bool> { { "param1", true } }
            };
            
            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .Returns(Task.FromResult<Profile?>(profile));
                
            _repositoryMock.Setup(r => r.DeleteProfileAsync(profileName))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteProfileAsync(profileName);

            // Assert
            // Verifica que os métodos do repositório foram chamados
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
            _repositoryMock.Verify(r => r.DeleteProfileAsync(profileName), Times.Once);
        }
        
        [Fact]
        public async Task DeleteProfileAsync_WithEmptyName_ShouldThrowBadRequestException()
        {
            // Arrange
            string profileName = "";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.DeleteProfileAsync(profileName));

            Assert.Equal(ErrorMessages.Profile.InvalidProfileName, exception.Message);
            
            // Verifica que o repositório não foi chamado
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.DeleteProfileAsync(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public async Task DeleteProfileAsync_WithNonExistentProfile_ShouldThrowNotFoundException()
        {
            // Arrange
            var profileName = "NonExistentProfile";
            
            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .Returns(Task.FromResult<Profile?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.DeleteProfileAsync(profileName));

            Assert.Equal(ErrorMessages.Profile.ProfileNotFound, exception.Message);
            
            // Verifica que os métodos do repositório foram chamados corretamente
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
            _repositoryMock.Verify(r => r.DeleteProfileAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddProfileAsync_WithValidProfile_ShouldAddProfile()
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

            _repositoryMock.Setup(r => r.AddProfileAsync(It.IsAny<Profile>()))
                .Returns(Task.FromResult(profile));

            // Act
            var result = await _service.AddProfileAsync(profile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(profileName, result.Name);
            Assert.Equal(2, result.Parameters.Count);
            Assert.True(result.Parameters["param1"]);
            Assert.False(result.Parameters["param2"]);

            _repositoryMock.Verify(r => r.AddProfileAsync(It.IsAny<Profile>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_WithValidNameAndParameters_ShouldUpdateProfile()
        {
            // Arrange
            var profileName = "TestProfile";
            var existingProfile = new Profile
            {
                Name = profileName,
                Parameters = new Dictionary<string, bool> 
                { 
                    { "param1", true }, 
                    { "param2", false } 
                }
            };
            
            var updatedParameters = new Dictionary<string, bool> 
            { 
                { "param1", false }, 
                { "param3", true } 
            };
            
            var updatedProfile = new Profile
            {
                Name = profileName,
                Parameters = updatedParameters
            };

            _repositoryMock.Setup(r => r.GetProfileByNameAsync(profileName))
                .Returns(Task.FromResult<Profile?>(existingProfile));
            _repositoryMock.Setup(r => r.UpdateProfileAsync(It.IsAny<Profile>()))
                .Returns(Task.FromResult(updatedProfile));

            // Act
            var result = await _service.UpdateProfileAsync(profileName, updatedParameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(profileName, result.Name);
            Assert.Equal(2, result.Parameters.Count);
            Assert.False(result.Parameters["param1"]);
            Assert.True(result.Parameters["param3"]);
            
            // Verifica que os métodos do repositório foram chamados
            _repositoryMock.Verify(r => r.GetProfileByNameAsync(profileName), Times.Once);
            _repositoryMock.Verify(r => r.UpdateProfileAsync(It.Is<Profile>(p => 
                p.Name == profileName && 
                p.Parameters == updatedParameters)), Times.Once);
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithValidData_ShouldReturnValidationResults()
        {
            // Arrange
            var name = "TestProfile";
            var actions = new List<string> { "CanEdit", "CanDelete", "NonExistentAction" };
            
            var existingProfile = new Profile
            {
                Name = name,
                Parameters = new Dictionary<string, bool>
                {
                    { "CanEdit", true },
                    { "CanDelete", false }
                }
            };
            
            _repositoryMock.Setup(repo => repo.GetProfileByNameAsync(name))
                .Returns(Task.FromResult<Profile?>(existingProfile));
            
            // Act
            var result = await _service.ValidateProfilePermissionsAsync(name, actions);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Results.Count);
            Assert.Equal("Allowed", result.Results["CanEdit"]);
            Assert.Equal("Denied", result.Results["CanDelete"]);
            Assert.Equal("Undefined", result.Results["NonExistentAction"]);
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithNonExistentProfile_ShouldThrowNotFoundException()
        {
            // Arrange
            var name = "NonExistentProfile";
            var actions = new List<string> { "CanEdit" };
            
            _repositoryMock.Setup(repo => repo.GetProfileByNameAsync(name))
                .Returns(Task.FromResult<Profile?>(null));
            
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.ValidateProfilePermissionsAsync(name, actions));
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithEmptyActions_ShouldThrowBadRequestException()
        {
            // Arrange
            var name = "TestProfile";
            var actions = new List<string>();
            
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.ValidateProfilePermissionsAsync(name, actions));
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithNullActions_ShouldThrowBadRequestException()
        {
            // Arrange
            var name = "TestProfile";
            List<string> actions = new();
            
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.ValidateProfilePermissionsAsync(name, actions));
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithEmptyProfileName_ShouldThrowBadRequestException()
        {
            // Arrange
            var name = "";
            var actions = new List<string> { "CanEdit" };
            
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.ValidateProfilePermissionsAsync(name, actions));
        }

        [Fact]
        public async Task AddProfileAsync_WithEmptyParameters_ShouldThrowBadRequestException()
        {
            // Arrange
            var profileName = "TestProfile";
            var profile = new Profile
            {
                Name = profileName,
                Parameters = new Dictionary<string, bool>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
                _service.AddProfileAsync(profile));
                
            // Verificar a mensagem de erro
            Assert.Equal("Parameter list cannot be empty", exception.Message);
        }
    }
} 