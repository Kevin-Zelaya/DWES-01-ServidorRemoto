using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

public class DependencyProvider
{
    public static ServiceProvider Configure()
    {
        var services = new ServiceCollection();

        // Refir
        services.AddHttpClient("jasonplaceholder", client =>
        {
            client.BaseAddress = new Uri(AppConfig.ApiUrl);
            client.DefaultRequestHeaders.Add("Acept", "application/jon");
        })
        .AddRefitClient<IUserApi>();


        services.AddScoped<IUserApiService, UserApiService>();
        return services.BuildServiceProvider();
    }
}