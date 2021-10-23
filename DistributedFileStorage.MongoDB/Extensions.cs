using DistributedFileStorage;
using DistributedFileStorage.MongoDB;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DfsEfcExtensions
    {
        public static IServiceCollection AddDfsMongo(this IServiceCollection services,
            Action<IServiceProvider, DfsMongoOptions>? optionsBuilder = null,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            return services.AddDfsMongo<string>(optionsBuilder, lifetime);
        }

        public static IServiceCollection AddDfsMongo<TMetadata>(this IServiceCollection services,
            Action<IServiceProvider, DfsMongoOptions>? optionsBuilder = null,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            services.Add(new ServiceDescriptor(typeof(DfsMongoOptions), x =>
            {
                var options = new DfsMongoOptions();
                optionsBuilder?.Invoke(x, options);
                return options;
            }, lifetime));

            services.AddTransient(x => x.GetRequiredService<DfsMongoOptions>().Database);
            services.AddTransient(x => x.GetRequiredService<DfsMongoOptions>().FileStorage);

            services.Add(new ServiceDescriptor(typeof(IDfsDatabase<TMetadata>), typeof(DfsDatabase<TMetadata>), lifetime));
            services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), typeof(DistributedFileStorage<TMetadata>), lifetime));

            return services;
        }
    }

}
