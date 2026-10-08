 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class UserSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<UserSyncBackgroundService> logger
) : BackgroundService
{
    public static event EventHandler<DatabaseRefreshedEventArgs>? DatabaseRefreshed;
    protected override async Task ExecuteAsync(
        CancellationToken cts)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(
                AppConfig.BatchSettings.IntervalInSeconds));
        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var userService = scope.ServiceProvider
                    .GetRequiredService<IUserService>();

                var result = await userService.SyncUsersAsync(cts);

                if (result.IsSuccess)
                {
                    DatabaseRefreshed?.Invoke(
                        this,
                        new DatabaseRefreshedEventArgs(
                            result.Value,
                            DateTime.UtcNow
                        )
                    );
                }
                else
                {
                    logger.LogError(
                        "Falló la sincronización de usuarios: {Error}",
                        result.Error);
                }
            }
            catch (OperationCanceledException)
                when (cts.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error inesperado durante la sincronización de usuarios.");
            }
        } while(await timer.WaitForNextTickAsync(cts));
    }
}