using DistributedFileStorage;
using DistributedFileStorage.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection;

public static class DfsEfcExtensions
{
    public static IServiceCollection AddDfsEfc(this IServiceCollection services,
        Action<IServiceProvider, DfsEfcOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddDfsEfc<string>(optionsBuilder, lifetime);
    }

    public static IServiceCollection AddDfsEfc<TMetadata>(this IServiceCollection services,
        Action<IServiceProvider, DfsEfcOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), (x) => CreateDfs<TMetadata>(x, optionsBuilder), lifetime));
        return services;
    }

    public static IServiceCollection AddKeyedDfsEfc(this IServiceCollection services,
        object? serviceKey,
        Action<IServiceProvider, DfsEfcOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddKeyedDfsEfc<string>(serviceKey, optionsBuilder, lifetime);
    }

    public static IServiceCollection AddKeyedDfsEfc<TMetadata>(this IServiceCollection services,
        object? serviceKey,
        Action<IServiceProvider, DfsEfcOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), serviceKey, (x, k) => CreateDfs<TMetadata>(x, optionsBuilder), lifetime));
        return services;
    }

    static DfsEfc<TMetadata> CreateDfs<TMetadata>(IServiceProvider x, Action<IServiceProvider, DfsEfcOptions>? optionsBuilder)
    {
        var options = new DfsEfcOptions();
        optionsBuilder?.Invoke(x, options);

        return new(new DfsDatabase<TMetadata>(options.Database), options.FileStorage);
    }
}
