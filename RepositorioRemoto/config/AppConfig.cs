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

    }

    public static IConfiguration Config {get; private set;}

    // Por aqui
    public static string ApiUrl {get; set;}

    public static string ConnectionString {get; private set;} 
}