//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Moq;
//using ValidProfiles.Application.DTOs;
//using ValidProfiles.Application.Interfaces;

//namespace ValidProfiles.Tests.BackgroundServices
//{
//    public class ProfileUpdateBackgroundServiceTests
//    {
//        private readonly Mock<IServiceProvider> _serviceProviderMock;
//        private readonly Mock<IServiceScope> _serviceScopeMock;
//        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
//        private readonly Mock<IProfileService> _profileServiceMock;

//        public ProfileUpdateBackgroundServiceTests()
//        {
//            _serviceProviderMock = new Mock<IServiceProvider>();
//            _serviceScopeMock = new Mock<IServiceScope>();
//            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
//            _profileServiceMock = new Mock<IProfileService>();

//            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
//            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
//            _serviceProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(_serviceScopeFactoryMock.Object);
//            _serviceProviderMock.Setup(p => p.GetService(typeof(IProfileService))).Returns(_profileServiceMock.Object);
//        }

//        [Fact]
//        public async Task StartAsync_ShouldCallUpdateProfileParameters()
//        {
//            // Arrange
//            var profilesResponse = new ProfilesResponseDto
//            {
//                Profiles = new List<ProfileResponseDto>
//                {
//                    new ProfileResponseDto
//                    {
//                        Name = "TestProfile",
//                        Parameters = new Dictionary<string, bool>
//                        {
//                            { "CanEdit", true },
//                            { "CanDelete", false }
//                        }
//                    }
//                }
//            };

//            _profileServiceMock.Setup(s => s.GetProfilesAsync())
//                .ReturnsAsync(profilesResponse);

//            // Usar um intervalo curto para o teste (100ms)
//                _serviceProviderMock.Object,
//                TimeSpan.FromMilliseconds(100));

//            // Act & Assert
//            // Criar um token de cancelamento que será cancelado após um curto período
//            using var cts = new CancellationTokenSource();
            
//            // Iniciar a tarefa do serviço
//            var task = backgroundService.StartAsync(cts.Token);
            
//            // Aguardar tempo suficiente para pelo menos uma execução
//            await Task.Delay(200);
            
//            // Cancelar a execução
//            cts.Cancel();
            
//            // Aguardar a conclusão da tarefa
//            await backgroundService.StopAsync(CancellationToken.None);
            
//            // Verificar se os métodos foram chamados
//            _serviceScopeFactoryMock.Verify(f => f.CreateScope(), Times.AtLeastOnce);
//            _profileServiceMock.Verify(s => s.GetProfilesAsync(), Times.AtLeastOnce);
//        }
//    }
//} 