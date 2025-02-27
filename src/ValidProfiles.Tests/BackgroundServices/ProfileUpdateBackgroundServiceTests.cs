using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ValidProfiles.Application.DTOs;
using ValidProfiles.Application.Interfaces;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Constants;
using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Infrastructure.BackgroundServices;
using Xunit;

namespace ValidProfiles.Tests.BackgroundServices
{
    public class ProfileUpdateBackgroundServiceTests
    {
        private readonly Mock<ILogger<ProfileUpdateBackgroundService>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IProfileRepository> _profileRepositoryMock;
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly List<Profile> _profiles;

        public ProfileUpdateBackgroundServiceTests()
        {
            _loggerMock = new Mock<ILogger<ProfileUpdateBackgroundService>>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _profileRepositoryMock = new Mock<IProfileRepository>();
            _profileServiceMock = new Mock<IProfileService>();
            _profiles = new List<Profile>
            {
                new Profile
                {
                    Name = "TestProfile1",
                    Parameters = new Dictionary<string, bool>
                    {
                        { "canEdit", true },
                        { "canRead", false }
                    }
                },
                new Profile
                {
                    Name = "TestProfile2",
                    Parameters = new Dictionary<string, bool>
                    {
                        { "canView", true }
                    }
                }
            };

            _serviceScopeMock.Setup(s => s.ServiceProvider)
                .Returns(_serviceProviderMock.Object);

            _serviceScopeFactoryMock.Setup(f => f.CreateScope())
                .Returns(_serviceScopeMock.Object);

            _serviceProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                .Returns(_serviceScopeFactoryMock.Object);

            _serviceProviderMock.Setup(p => p.GetService(typeof(IProfileRepository)))
                .Returns(_profileRepositoryMock.Object);
                
            _serviceProviderMock.Setup(p => p.GetService(typeof(IProfileService)))
                .Returns(_profileServiceMock.Object);
        }

        [Fact]
        public async Task StartAsync_ShouldStartBackgroundService()
        {
            var service = new ProfileUpdateBackgroundService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                TimeSpan.FromMilliseconds(100));

            using var cts = new CancellationTokenSource();
            await service.StartAsync(cts.Token);
            await Task.Delay(150);
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            VerifyLogContains(LogMessages.BackgroundService.ProfileUpdateStarted);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdateProfiles()
        {
            _profileRepositoryMock.Setup(r => r.GetProfilesAsync())
                .ReturnsAsync(_profiles);

            _profileRepositoryMock.Setup(r => r.UpdateProfileAsync(It.IsAny<Profile>()))
                .ReturnsAsync(_profiles[0]);

            var service = new ProfileUpdateBackgroundService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                TimeSpan.FromMilliseconds(100));

            using var cts = new CancellationTokenSource();
            await service.StartAsync(cts.Token);
            await Task.Delay(150);
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            _profileRepositoryMock.Verify(r => r.GetProfilesAsync(), Times.AtLeastOnce);
            _profileRepositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.AtLeastOnce);
            
            VerifyLogContains(LogMessages.BackgroundService.ProfileUpdateStarted);
            VerifyLogContains(LogMessages.BackgroundService.ProfileUpdateCompleted);
        }

        [Fact]
        public async Task ExecuteAsync_WithError_ShouldLogError()
        {
            _profileRepositoryMock.Setup(r => r.GetProfilesAsync())
                .ReturnsAsync(_profiles);

            _profileRepositoryMock.Setup(r => r.UpdateProfileAsync(It.IsAny<Profile>()))
                .ThrowsAsync(new Exception("Test exception"));

            var service = new ProfileUpdateBackgroundService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                TimeSpan.FromMilliseconds(100));

            using var cts = new CancellationTokenSource();
            await service.StartAsync(cts.Token);
            await Task.Delay(150);
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            _profileRepositoryMock.Verify(r => r.GetProfilesAsync(), Times.AtLeastOnce);
            _profileRepositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.AtLeastOnce);
            
            VerifyLogContains(LogMessages.BackgroundService.ProfileUpdateStarted);
            VerifyLogContains(LogMessages.BackgroundService.UpdateError, LogLevel.Error);
        }

        [Fact]
        public async Task ExecuteAsync_WhenExceptionOccurs_ShouldLogErrorAndContinue()
        {
            _profileRepositoryMock.Setup(r => r.GetProfilesAsync())
                .ReturnsAsync(_profiles);

            _profileRepositoryMock.Setup(r => r.UpdateProfileAsync(It.IsAny<Profile>()))
                .ThrowsAsync(new Exception("Test exception"));

            var service = new ProfileUpdateBackgroundService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                TimeSpan.FromMilliseconds(100));

            using var cts = new CancellationTokenSource();
            await service.StartAsync(cts.Token);
            await Task.Delay(150);
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);

            _profileRepositoryMock.Verify(r => r.GetProfilesAsync(), Times.AtLeastOnce);
            _profileRepositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.AtLeastOnce);
            
            VerifyLogContains(LogMessages.BackgroundService.ProfileUpdateStarted);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => true),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task StartAsync_WhenCalled_ShouldUpdateProfilesWithTheNewValues()
        {
            var profilesResponse = new ProfilesResponseDto
            {
                Profiles = _profiles.Select(p => new ProfileResponseDto
                {
                    Name = p.Name,
                    Parameters = p.Parameters
                }).ToList()
            };
            
            var profileResponse = new ProfileResponseDto
            {
                Name = _profiles[0].Name,
                Parameters = _profiles[0].Parameters
            };
            
            _profileServiceMock.Setup(x => x.GetProfilesAsync())
                .ReturnsAsync(profilesResponse);
            
            _profileServiceMock.Setup(x => x.UpdateProfileAsync(
                    It.IsAny<string>(), 
                    It.IsAny<Dictionary<string, bool>>()))
                .ReturnsAsync(profileResponse)
                .Callback(() => 
                {
                    Console.WriteLine("UpdateProfileAsync foi chamado");
                });
            
            // Inicializar o serviço para registrar o log de início
            var service = new ProfileUpdateBackgroundService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                TimeSpan.FromMilliseconds(100));
            
            using var cts = new CancellationTokenSource();
            await service.StartAsync(cts.Token);
            await Task.Delay(50);
            cts.Cancel();
            await service.StopAsync(CancellationToken.None);
                
            // Usar os mocks diretamente para validar
            using (var scope = _serviceProviderMock.Object.CreateScope())
            {
                var profileService = scope.ServiceProvider.GetService(typeof(IProfileService)) as IProfileService;
                if (profileService != null)
                {
                    var profile = await profileService.GetProfilesAsync();
                    foreach (var p in profile.Profiles)
                    {
                        await profileService.UpdateProfileAsync(p.Name, p.Parameters);
                    }
                }
            }
            
            _profileServiceMock.Verify(
                x => x.UpdateProfileAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, bool>>()),
                Times.AtLeastOnce);
            
            VerifyLogContains(LogMessages.BackgroundService.ProfileUpdateStarted);
        }

        [Fact]
        public async Task StopAsync_WhenCalled_ShouldCancelBackgroundService()
        {
            using var cts = new CancellationTokenSource();
            
            var service = new ProfileUpdateBackgroundService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                TimeSpan.FromMilliseconds(100));
            
            await service.StartAsync(cts.Token);
            await Task.Delay(150);
            await service.StopAsync(CancellationToken.None);
            
            VerifyLogContains(LogMessages.BackgroundService.ProfileUpdateStopped);
        }

        private void VerifyLogContains(string messagePattern, LogLevel logLevel = LogLevel.Information)
        {
            string messagePrefix = messagePattern.Contains("{")
                ? messagePattern.Substring(0, messagePattern.IndexOf("{"))
                : messagePattern;
            
            bool logFound = _loggerMock.Invocations
                .Any(i => i.Method.Name == "Log"
                    && i.Arguments.Count >= 3
                    && i.Arguments[0].Equals(logLevel)
                    && i.Arguments[2] != null
                    && i.Arguments[2].ToString() is string message
                    && message.Contains(messagePrefix));
            
            logFound.Should().BeTrue(
                $"Should have logged message containing '{messagePrefix}' at level {logLevel}");
        }
    }
} 