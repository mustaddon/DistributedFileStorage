using DistributedFileStorage;
using DistributedFileStorage.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DfsEfcExtensions
    {
        public static IServiceCollection AddDfsEfc(this IServiceCollection services,
            Action<IServiceProvider, DfsEfcOptions> optionsBuilder, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            return services.AddDfsEfc<string>(optionsBuilder, lifetime);
        }

        public static IServiceCollection AddDfsEfc<TMetadata>(this IServiceCollection services,
            Action<IServiceProvider, DfsEfcOptions> optionsBuilder,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            services.Add(new ServiceDescriptor(typeof(DfsEfcOptions), x =>
            {
                var options = new DfsEfcOptions();
                optionsBuilder(x, options);
                return options;
            }, lifetime));

            services.AddTransient(x => x.GetRequiredService<DfsEfcOptions>().Database);
            services.AddTransient(x => x.GetRequiredService<DfsEfcOptions>().FileStorage);

            services.Add(new ServiceDescriptor(typeof(IDfsDatabase<TMetadata>), typeof(DfsDatabase<TMetadata>), lifetime));
            services.Add(new ServiceDescriptor(typeof(IDistributedFileStorage<TMetadata>), typeof(DistributedFileStorage<TMetadata>), lifetime));

            return services;
        }
    }

}
