using DistributedFileStorage;
using DistributedFileStorage.MongoDB;
using System;

namespace Microsoft.Extensions.DependencyInjection;

public static class DfsMongoExtensions
{
    public static IServiceCollection AddDfsMongo(this IServiceCollection services,
        Action<IServiceProvider, DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddDfsMongo<string>(optionsBuilder, lifetime);
    }

    public static IServiceCollection AddDfsMongo<TMetadata>(this IServiceCollection services,
        Action<IServiceProvider, DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), (x) => CreateDfs<TMetadata>(x, optionsBuilder), lifetime));
        return services;
    }
    public static IServiceCollection AddKeyedDfsMongo(this IServiceCollection services,
        object? serviceKey,
        Action<IServiceProvider, DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddKeyedDfsMongo<string>(serviceKey, optionsBuilder, lifetime);
    }

    public static IServiceCollection AddKeyedDfsMongo<TMetadata>(this IServiceCollection services,
        object? serviceKey,
        Action<IServiceProvider, DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), serviceKey, (x, k) => CreateDfs<TMetadata>(x, optionsBuilder), lifetime));
        return services;
    }

    static DfsMongo<TMetadata> CreateDfs<TMetadata>(IServiceProvider x, Action<IServiceProvider, DfsMongoOptions>? optionsBuilder)
    {
        var options = new DfsMongoOptions();
        optionsBuilder?.Invoke(x, options);

        return new(new DfsDatabase<TMetadata>(options.Database), options.FileStorage);
    }
}
