using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Configuración de la aplicación
var builder = Host.CreateApplicationBuilder(args);

// Reducir los logs de HttpClient y Entity Framework
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

DependencyProvider.Configure(
    builder.Services,
    builder.Environment
);

using var host = builder.Build();


UserSyncBackgroundService.DatabaseRefreshed += (sender, e) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[CONSOLA] Base de datos sincronizada. Total registros: {e.RegistrosCargados}");
    Console.ResetColor();
};

await host.StartAsync();



try
{
    // IUserApiService está registrado como Scoped
    using var scope = host.Services.CreateScope();

    var userService = scope.ServiceProvider
        .GetRequiredService<UserService>();

    var users = await userService.GetAllUsersAsync();

    foreach (var user in users.Value)
    {
        Console.WriteLine(user.name);
    }
    await userService.GetUserByIdAsync(1);
    Task.Delay(1000);
    await userService.GetUserByIdAsync(1);

    Console.ReadLine();
}
finally
{
    await host.StopAsync();
}


// Entender toda está parte y la inyección de dependencias y la configuración
// de entorno o  perfil