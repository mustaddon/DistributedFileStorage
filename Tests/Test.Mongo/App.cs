using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Test.Common;

namespace Test.Mongo
{
    internal class App
    {
        public static Lazy<IHost> Instance = new(static () =>
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDfsMongo<TestMetadata>();
                });

            return builder.Build();
        });

    }
}
