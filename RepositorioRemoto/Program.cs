


// Prueba

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var provider = DependencyProvider.Configure();

UserSyncBackgroundService.DatabaseRefreshed += (sender, e) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[OYENTE CONSOLA] 🔔 Base de datos sincronizada. Total registros: {e.RegistrosCargados}");
    Console.ResetColor();
};

// 💡 3. Arrancar el BackgroundService usando el mismo ServiceProvider que ya configuraste
var backgroundService = provider.GetRequiredService<IHostedService>();
using var cts = new CancellationTokenSource();
_ = backgroundService.StartAsync(cts.Token);


var services = provider.GetRequiredService<IUserApiService>();


var users = await services.GetAllAsync();


foreach(var user in users.Value)
{
    Console.WriteLine($"{user.name}");
}
Console.ReadLine();
await backgroundService.StopAsync(cts.Token);

