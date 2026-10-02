using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Refit;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Cache.Common;

public class DependencyProvider
{
    public static ServiceProvider Configure()
    {
        var services = new ServiceCollection();

        // Registro nativo de IMemoryCache de Microsoft
        services.AddMemoryCache();

        // Registro de tu InMemoryCache implementando IPostService como Singleton
        services.AddSingleton<IPostService, InMemoryCache>();

        // Refit
        services.AddHttpClient("jasonplaceholder", client =>
            {
                client.BaseAddress = new Uri(AppConfig.ApiUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddRefitClient<IUserApi>();

        services.AddScoped<IUserApiService, UserApiService>();

        return services.BuildServiceProvider();
    }
}