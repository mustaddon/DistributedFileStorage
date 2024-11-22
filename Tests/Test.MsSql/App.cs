using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Test.Common;

namespace Test.EFCore
{
    internal class App
    {
        public static Lazy<IHost> Instance = new(static () =>
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDfsEfc<TestMetadata>(options =>
                    {
                        options.Database.ContextConfigurator = (db) => db.UseSqlServer(hostContext.Configuration.GetConnectionString("dfs"));
                    });
                });

            return builder.Build();
        });

    }
}
