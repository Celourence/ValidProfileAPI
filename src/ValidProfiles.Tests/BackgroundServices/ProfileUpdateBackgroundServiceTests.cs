using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ValidProfiles.Domain;
using ValidProfiles.Domain.Interfaces;
using ValidProfiles.Infrastructure.BackgroundServices;

namespace ValidProfiles.Tests.BackgroundServices
{
    public class ProfileUpdateBackgroundServiceTests
    {
        private readonly Mock<ILogger<ProfileUpdateBackgroundService>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IProfileRepository> _repositoryMock;
        private readonly Mock<IProfileCache> _cacheMock;

        public ProfileUpdateBackgroundServiceTests()
        {
            _loggerMock = new Mock<ILogger<ProfileUpdateBackgroundService>>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _repositoryMock = new Mock<IProfileRepository>();
            _cacheMock = new Mock<IProfileCache>();

            // Configurar o escopo de serviço
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);
            
            // Configurar os serviços para serem resolvidos via GetRequiredService
            _serviceProviderMock.Setup(p => p.GetService(typeof(IProfileRepository))).Returns(_repositoryMock.Object);
            _serviceProviderMock.Setup(p => p.GetService(typeof(IProfileCache))).Returns(_cacheMock.Object);
        }

        [Fact]
        public async Task StartAsync_ShouldCallUpdateProfileParameters()
        {
            // Arrange
            var profiles = new List<Profile>
            {
                new Profile
                {
                    Name = "TestProfile",
                    Parameters = new Dictionary<string, bool>
                    {
                        { "CanEdit", true },
                        { "CanDelete", false }
                    }
                }
            };

            _repositoryMock.Setup(r => r.GetProfilesAsync())
                .Returns(Task.FromResult<IEnumerable<Profile>>(profiles));

            _repositoryMock.Setup(r => r.UpdateProfileAsync(It.IsAny<Profile>()))
                .Returns((Profile p) => Task.FromResult(p));

            // Usar um intervalo curto para o teste (100ms)
            var backgroundService = new ProfileUpdateBackgroundService(
                _loggerMock.Object,
                _serviceProviderMock.Object,
                TimeSpan.FromMilliseconds(100));

            // Act & Assert
            // Criar um token de cancelamento que será cancelado após um curto período
            using var cts = new CancellationTokenSource();
            
            // Iniciar a tarefa do serviço
            var task = backgroundService.StartAsync(cts.Token);
            
            // Aguardar tempo suficiente para pelo menos uma execução
            await Task.Delay(200);
            
            // Cancelar a execução
            cts.Cancel();
            
            // Aguardar a conclusão da tarefa
            await backgroundService.StopAsync(CancellationToken.None);
            
            // Verificar se os métodos foram chamados
            _serviceScopeFactoryMock.Verify(f => f.CreateScope(), Times.AtLeastOnce);
            _repositoryMock.Verify(r => r.GetProfilesAsync(), Times.AtLeastOnce);
            _repositoryMock.Verify(r => r.UpdateProfileAsync(It.IsAny<Profile>()), Times.AtLeastOnce);
        }
    }
} 