using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ValidProfiles.API.Controllers;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain.Exceptions;

namespace ValidProfiles.Tests
{
    public class ProfileControllerTests
    {
        private readonly Mock<IProfileService> _serviceMock;
        private readonly Mock<ILogger<ProfileController>> _loggerMock;
        private readonly ProfileController _controller;

        public ProfileControllerTests()
        {
            _serviceMock = new Mock<IProfileService>();
            _loggerMock = new Mock<ILogger<ProfileController>>();
            _controller = new ProfileController(_serviceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithValidData_ShouldReturnOkResult()
        {
            // Arrange
            var profileName = "TestProfile";
            var request = new ValidationRequestDto
            {
                Actions = new List<string> { "CanEdit", "CanDelete", "NonExistentAction" }
            };
            
            var expectedResponse = new ValidationResponseDto
            {
                ProfileName = "TestProfile",
                Results = new Dictionary<string, string>
                {
                    { "CanEdit", "Allowed" },
                    { "CanDelete", "Denied" },
                    { "NonExistentAction", "Undefined" }
                }
            };
            
            _serviceMock.Setup(service => service.ValidateProfilePermissionsAsync(
                    profileName, 
                    It.IsAny<List<string>>()))
                .ReturnsAsync(expectedResponse);
            
            // Act
            var result = await _controller.ValidateProfilePermissionsAsync(profileName, request);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ValidationResponseDto>(okResult.Value);
            
            Assert.Equal(3, returnValue.Results.Count);
            Assert.Equal("Allowed", returnValue.Results["CanEdit"]);
            Assert.Equal("Denied", returnValue.Results["CanDelete"]);
            Assert.Equal("Undefined", returnValue.Results["NonExistentAction"]);
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithNonExistentProfile_ShouldThrowNotFoundException()
        {
            // Arrange
            var profileName = "NonExistentProfile";
            var request = new ValidationRequestDto
            {
                Actions = new List<string> { "CanEdit" }
            };
            
            _serviceMock.Setup(service => service.ValidateProfilePermissionsAsync(
                    profileName, 
                    It.IsAny<List<string>>()))
                .ThrowsAsync(new NotFoundException("Perfil não encontrado"));
            
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _controller.ValidateProfilePermissionsAsync(profileName, request));
        }

        [Fact]
        public async Task ValidateProfilePermissionsAsync_WithBadRequest_ShouldThrowBadRequestException()
        {
            // Arrange
            var profileName = "TestProfile";
            var request = new ValidationRequestDto
            {
                Actions = new List<string>()
            };
            
            _serviceMock.Setup(service => service.ValidateProfilePermissionsAsync(
                    profileName, 
                    It.IsAny<List<string>>()))
                .ThrowsAsync(new BadRequestException("Action list cannot be empty"));
            
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => 
                _controller.ValidateProfilePermissionsAsync(profileName, request));
        }
    }
} 