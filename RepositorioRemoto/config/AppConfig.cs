using Microsoft.Extensions.Configuration;

public class AppConfig
{
    

    static ApiConfig()
    {
        var enviroment =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        Config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true
            )
            .AddJsonFile(
                $"appsettings{enviroment}.json",
                optional: false,
                reloadOnChange: true
            )
            .Build();



    }

    public static IConfiguration Config {get; private set;}

    // Por aqui
}