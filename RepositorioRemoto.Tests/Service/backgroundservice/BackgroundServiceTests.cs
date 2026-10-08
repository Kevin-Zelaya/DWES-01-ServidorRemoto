
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

public class BackgroundServiceTests
{
    private Mock<IServiceScopeFactory> _scopeFactory = null!;
    private Mock<IServiceScope> _scope = null!;
    private Mock<IServiceProvider> _serviceProvider = null!;
    private Mock<IUserService> _userService = null!;
    private Mock<ILogger<UserSyncBackgroundService>> _logger = null!;


    [SetUp]
    public void Setup()
    {
        _scopeFactory = new Mock<IServiceScopeFactory>();
        _scope = new Mock<IServiceScope>();
        _serviceProvider = new Mock<IServiceProvider>();
        _userService = new Mock<IUserService>();
        _logger = new Mock<ILogger<UserSyncBackgroundService>>();

        _scopeFactory
            .Setup(x => x.CreateScope())
            .Returns(_scope.Object);

        _scope
            .Setup(x => x.ServiceProvider)
            .Returns(_serviceProvider.Object);

        _serviceProvider
            .Setup(x => x.GetService(typeof(IUserService)))
            .Returns(_userService.Object);
    }
    [Test]
    public async Task ExecuteAsync_WhenSyncFails_ShouldLogError()
    {
        var error = new DatabaseError.WriteFailure(
            "Error sincronizando usuarios");

        _userService
            .Setup(x => x.SyncUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<int, DomainError>(error));

        var service = new UserSyncBackgroundService(
            _scopeFactory.Object,
            _logger.Object);

        using var cts = new CancellationTokenSource();

        cts.CancelAfter(100);

        await service.StartAsync(cts.Token);
        await service.StopAsync(CancellationToken.None);

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (state, type) =>
                        state.ToString()!.Contains(
                            "Falló la sincronización de usuarios")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
    [Test]
    public async Task ExecuteAsync_WhenSyncIsCancelled_ShouldStopWithoutError()
    {
        using var cts = new CancellationTokenSource();

        _userService
            .Setup(x => x.SyncUsersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = new UserSyncBackgroundService(
            _scopeFactory.Object,
            _logger.Object);

        await service.StartAsync(cts.Token);

        cts.Cancel();

        await service.StopAsync(CancellationToken.None);

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}