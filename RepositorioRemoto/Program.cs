
// Configuración de la aplicación

using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Solo para limpiar los logs
// builder.Logging.ClearProviders();

// builder.Logging.AddFilter(
//     "Microsoft.EntityFrameworkCore.Database.Command",
//     LogLevel.None);

// builder.Logging.AddFilter(
//     "Microsoft.EntityFrameworkCore.Query",
//     LogLevel.None);

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

// documentación swagger

app.MapOpenApi();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/openapi/v1.json", "Mi API V1");
});

// Background service
UserSyncBackgroundService.DatabaseRefreshed += (sender, e) =>
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n[CONSOLA] Base de datos sincronizada. Total registros: {e.RegistrosCargados}");
    Console.ResetColor();
};

// Controllers
app.MapControllers();


app.Run();