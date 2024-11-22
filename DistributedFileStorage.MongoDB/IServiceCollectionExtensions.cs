using DistributedFileStorage;
using DistributedFileStorage.MongoDB;
using System;

namespace Microsoft.Extensions.DependencyInjection;

public static class DfsMongoExtensions
{
    public static IServiceCollection AddDfsMongo(this IServiceCollection services,
        DfsMongoOptions options,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddDfsMongo<string>(options, lifetime);
    }

    public static IServiceCollection AddDfsMongo(this IServiceCollection services,
        Action<DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddDfsMongo<string>(optionsBuilder, lifetime);
    }

    public static IServiceCollection AddDfsMongo(this IServiceCollection services,
        Action<IServiceProvider, DfsMongoOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddDfsMongo<string>(optionsBuilder, lifetime);
    }

    public static IServiceCollection AddDfsMongo<TMetadata>(this IServiceCollection services,
        DfsMongoOptions options,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), (x) => CreateDfs<TMetadata>(options), lifetime));
        return services;
    }

    public static IServiceCollection AddDfsMongo<TMetadata>(this IServiceCollection services,
        Action<DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var options = new DfsMongoOptions();
        optionsBuilder?.Invoke(options);
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), (x) => CreateDfs<TMetadata>(options), lifetime));
        return services;
    }

    public static IServiceCollection AddDfsMongo<TMetadata>(this IServiceCollection services,
        Action<IServiceProvider, DfsMongoOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), (x) => CreateDfs<TMetadata>(x, optionsBuilder), lifetime));
        return services;
    }



    public static IServiceCollection AddKeyedDfsMongo(this IServiceCollection services,
        object? serviceKey,
        DfsMongoOptions options,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddKeyedDfsMongo<string>(serviceKey, options, lifetime);
    }

    public static IServiceCollection AddKeyedDfsMongo(this IServiceCollection services,
        object? serviceKey,
        Action<DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddKeyedDfsMongo<string>(serviceKey, optionsBuilder, lifetime);
    }

    public static IServiceCollection AddKeyedDfsMongo(this IServiceCollection services,
        object? serviceKey,
        Action<IServiceProvider, DfsMongoOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddKeyedDfsMongo<string>(serviceKey, optionsBuilder, lifetime);
    }

    public static IServiceCollection AddKeyedDfsMongo<TMetadata>(this IServiceCollection services,
        object? serviceKey,
        DfsMongoOptions options,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), serviceKey, (x, k) => CreateDfs<TMetadata>(options), lifetime));
        return services;
    }

    public static IServiceCollection AddKeyedDfsMongo<TMetadata>(this IServiceCollection services,
        object? serviceKey,
        Action<DfsMongoOptions>? optionsBuilder = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var options = new DfsMongoOptions();
        optionsBuilder?.Invoke(options);
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), serviceKey, (x, k) => CreateDfs<TMetadata>(options), lifetime));
        return services;
    }

    public static IServiceCollection AddKeyedDfsMongo<TMetadata>(this IServiceCollection services,
        object? serviceKey,
        Action<IServiceProvider, DfsMongoOptions> optionsBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), serviceKey, (x, k) => CreateDfs<TMetadata>(x, optionsBuilder), lifetime));
        return services;
    }



    static DfsMongo<TMetadata> CreateDfs<TMetadata>(IServiceProvider x, Action<IServiceProvider, DfsMongoOptions>? optionsBuilder)
    {
        var options = new DfsMongoOptions();
        optionsBuilder?.Invoke(x, options);
        return CreateDfs<TMetadata>(options);
    }

    static DfsMongo<TMetadata> CreateDfs<TMetadata>(DfsMongoOptions options)
    {
        return new(new DfsDatabase<TMetadata>(options.Database), options.FileStorage);
    }
}
