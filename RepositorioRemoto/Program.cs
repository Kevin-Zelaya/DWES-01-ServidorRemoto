


// Prueba



using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;



var provider = DependencyProvider.Configure();

UserSyncBackgroundService.DatabaseRefreshed += (sender, e) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[OYENTE CONSOLA] 🔔 Base de datos sincronizada. Total registros: {e.RegistrosCargados}");
    Console.ResetColor();
};

var backgroundService = provider.GetRequiredService<IHostedService>();
using var cts = new CancellationTokenSource();
_ = backgroundService.StartAsync(cts.Token);

var scoped = provider.CreateScope();

var userService = scoped.ServiceProvider.GetRequiredService<UserService>();

userService.SyncUsersAsync().GetAwaiter().GetResult();


Console.ReadLine();
await backgroundService.StopAsync(cts.Token);