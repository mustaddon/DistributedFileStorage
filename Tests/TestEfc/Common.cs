using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Text;

namespace TestEfc
{
    internal class Common
    {
        public static Lazy<IHost> App = new Lazy<IHost>(static () =>
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDfsEfc<TestMetadata>((sp, options) =>
                    {
                        options.Database.ContextConfigurator = (db) => db.UseSqlServer(hostContext.Configuration.GetConnectionString("dfs"));
                    });
                });

            return builder.Build();
        });

        public static Random Rnd = new();

        public static TestFile GenerateFile(string line = "test", int? count = null)
        {
            return new TestFile
            {
                Name = $"test_{DateTime.Now.Ticks}.txt",
                Content = Encoding.UTF8.GetBytes(string.Join("\n", Enumerable.Range(0, count ?? Rnd.Next(1, 5000)).Select(i => $"{i + 1}: {line}"))),
                Metadata = new TestMetadata { Text = $"text" },
            };
        }
    }

    class TestMetadata
    {
        public string Text { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

        public override int GetHashCode() => HashCode.Combine(Text, Date);
        public override bool Equals(object obj) => GetHashCode() == (obj as TestMetadata)?.GetHashCode();
    }

    class TestFile
    {
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public TestMetadata Metadata { get; set; }
    }

}
