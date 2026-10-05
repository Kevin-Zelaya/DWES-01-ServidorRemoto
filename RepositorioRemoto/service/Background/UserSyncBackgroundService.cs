 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class UserSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<UserSyncBackgroundService> logger
) : BackgroundService
{
    public static event EventHandler<DatabaseRefreshedEventArgs>?
        DatabaseRefreshed;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(
                AppConfig.BatchSettings.IntervalInSeconds));

        
        do{
            try
            {
                using var scope = scopeFactory.CreateScope();

                var userService = scope.ServiceProvider
                    .GetRequiredService<UserService>();

                var result = await userService.SyncUsersAsync(stoppingToken);

                if (result.IsSuccess)
                {
                    DatabaseRefreshed?.Invoke(
                        this,
                        new DatabaseRefreshedEventArgs(
                            result.Value,
                            DateTime.UtcNow));
                }
                else
                {
                    logger.LogError(
                        "Falló la sincronización de usuarios: {Error}",
                        result.Error);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error inesperado durante la sincronización de usuarios.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}