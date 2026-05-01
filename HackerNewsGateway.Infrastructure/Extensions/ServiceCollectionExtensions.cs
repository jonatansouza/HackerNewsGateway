using HackerNewsGateway.Domain.Interfaces;
using HackerNewsGateway.Domain.Options;
using HackerNewsGateway.Infrastructure.Cache;
using HackerNewsGateway.Infrastructure.Http;
using HackerNewsGateway.Infrastructure.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HackerNewsGateway.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHackerNewsOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HackerNewsOptions>(configuration.GetSection("HackerNews"));
        return services;
    }

    public static IServiceCollection AddHackerNewsHttpClient(
        this IServiceCollection services)
    {
        services.AddHttpClient<HackerNewsClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        return services;
    }

    public static IServiceCollection AddHackerNewsCache(
        this IServiceCollection services)
    {
        services.AddSingleton<IStoryCache, StoryCache>();
        return services;
    }

    public static IServiceCollection AddHackerNewsSyncWorker(
        this IServiceCollection services)
    {
        services.AddHostedService<StorySyncWorker>();
        return services;
    }
}
