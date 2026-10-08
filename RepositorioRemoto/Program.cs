
// Configuración de la aplicación

using Microsoft.EntityFrameworkCore.Storage;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Solo para limpiar los logs
builder.Logging.ClearProviders();

builder.Logging.AddFilter(
    "Microsoft.EntityFrameworkCore.Database.Command",
    LogLevel.None);

builder.Logging.AddFilter(
    "Microsoft.EntityFrameworkCore.Query",
    LogLevel.None);

builder.Logging.AddFilter(
    "Microsoft.EntityFrameworkCore.Database.Transaction",
    LogLevel.Information);

builder.Logging.AddFilter(
    "Microsoft.EntityFrameworkCore.Database.Connection",
    LogLevel.None);

builder.Logging.AddFilter(
    "Microsoft.EntityFrameworkCore.Database.Connection",
    LogLevel.None);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(AppConfig.Config)
    .CreateLogger();
// --------------------------------------------
builder.Logging.AddSerilog(Log.Logger);

DependencyProvider.Configure(
    builder.Services,
    builder.Environment
);
// Swagger / openapi
builder.Services.AddOpenApi();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Crear la base de datos SQLite si no existe
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    context.EnsureCreated();
}

// documentación swagger

app.MapOpenApi();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/openapi/v1.json", "Mi API V1");
});

// --------- Suscripciones a eventos ------------
// Background service
UserSyncBackgroundService.DatabaseRefreshed += (sender, e) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[CONSOLA] Base de datos sincronizada. Total registros: {e.RegistrosCargados}");
    Console.ResetColor();
};
var notificationService =
    app.Services.GetRequiredService<INotificationService>();
// create
notificationService.UserCreated += (sender, user) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[CONSOLA] Usuario creado: {user.id} {user.name}");
    Console.ResetColor();
};
// Update
notificationService.UserUpdated += (sender, user) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[CONSOLA] Usuario actualizado: {user.id} {user.name}");
    Console.ResetColor();
};
// Delete
notificationService.UserDeleted += (sender, id) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[CONSOLA] Eliminado usuario con el id: {id}");
    Console.ResetColor();
};

// Controllers
app.MapControllers();


app.Run();