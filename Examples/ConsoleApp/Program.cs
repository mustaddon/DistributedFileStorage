using ConsoleApp;
using DistributedFileStorage.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;


var app = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // add services to the container
        services.AddDfsEfc<MyCustomMetadata>((sp, options) =>
        {
            // add database provider 
            options.Database.ContextConfigurator = (db) => db.UseSqlServer(hostContext.Configuration.GetConnectionString("dfs"));

            // add path construction algorithm 
            var rnd = new Random();
            options.FileStorage.PathBuilder = (fileId) => Path.GetFullPath($@"_dfs\fake_disk_{rnd.Next(1, 3)}\{DateTime.Now:yyyy\\MM\\dd}\{fileId}");
        });
    })
    .Build();


var dfs = app.Services.GetRequiredService<IDistributedFileStorage<MyCustomMetadata>>();

// upload
using var uploadFile = Example.GenerateTextFileStream();
var fileId = await dfs.Add(uploadFile, $"example.txt", new MyCustomMetadata { Author = "User" });

// get info
var fileInfo = await dfs.GetInfo(fileId);
Console.WriteLine(JsonConvert.SerializeObject(fileInfo, Formatting.Indented));

// download 
using var downloadFile = File.Create("example.txt");
await foreach (var chunk in dfs.GetContent(fileId))
    await downloadFile.WriteAsync(chunk);
