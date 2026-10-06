
// Configuración de la aplicación

var builder = WebApplication.CreateBuilder(args);

// Reducir los logs de HttpClient y Entity Framework
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

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