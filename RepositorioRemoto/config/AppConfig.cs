using Microsoft.Extensions.Configuration;

public class AppConfig
{
    

    static AppConfig()
    {
        var enviroment =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "development";

        Config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true
            )
            .AddJsonFile(
                $"appsettings.{enviroment}.json",
                optional: false,
                reloadOnChange: true
            )
            .Build();

            ApiUrl = Config["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("No se encontro 'ApiSettins:BaseUrl'");
            ConnectionString = Config["ConnectionStrings:DefaultConnection"]
                ?? throw new InvalidOperationException("No se encontro 'ConnectionStrings:DefaultConnection'");
            BatchSettings = (
                BatchSize: int.TryParse(
                    Config["SincronizateSettings:BatchSize"], out var batchSize)
                        ? batchSize
                        : throw new InvalidOperationException(
                            "No se encontró 'SincronizateSettings:BatchSize'"),

                IntervalInSeconds: int.TryParse(
                    Config["SincronizateSettings:IntervalInSeconds"], out var interval)
                        ? interval
                        : throw new InvalidOperationException(
                            "No se encontró 'SincronizateSettings:IntervalInSeconds'")
            );
    }

    public static IConfiguration Config {get; private set;}

    // Por aqui
    public static string ApiUrl {get; set;}

    public static string ConnectionString {get; private set;} 

    public static (int BatchSize, int IntervalInSeconds) BatchSettings {get; private set;}
}