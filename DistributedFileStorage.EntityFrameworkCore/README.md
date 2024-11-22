# DistributedFileStorage.EntityFrameworkCore [![NuGet version](https://badge.fury.io/nu/DistributedFileStorage.EntityFrameworkCore.svg)](http://badge.fury.io/nu/DistributedFileStorage.EntityFrameworkCore)
DistributedFileStorage with EntityFrameworkCore implementation of IDfsDatabase


## Features
* Storing metadata in the database (SQL/NoSQL)
* Storing files in the file system
* Deduplication of files by content
* Distributed storage (multiple disks)



## Example: Simple console app
```
dotnet new console --name "DfsExample"
cd DfsExample
dotnet add package DistributedFileStorage.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

program.cs:
```C#
using DistributedFileStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


// add services to the container
var services = new ServiceCollection()
    .AddDfsEfc<string>(options =>
    {
        // add database provider 
        options.Database.ContextConfigurator = (db) => db.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DfsDatabase;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False");

        // add path construction algorithm 
        var rnd = new Random();
        options.FileStorage.PathBuilder = (fileId) => Path.GetFullPath($@"_dfs\fake_disk_{rnd.Next(1, 3)}\{DateTime.Now:yyyy\\MM\\dd}\{fileId}");
    })
    .BuildServiceProvider();


var dfs = services.GetRequiredService<IDistributedFileStorage<string>>();

// upload
using var uploadFile = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("test text"));
var fileId = await dfs.Add(uploadFile, $"example.txt", "my metadata");

// get info
var fileInfo = await dfs.GetInfo(fileId);
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(fileInfo));

// download 
using var downloadFile = File.Create("example.txt");
await foreach (var chunk in dfs.GetContent(fileId))
    await downloadFile.WriteAsync(chunk);
```


[More examples...](https://github.com/mustaddon/DistributedFileStorage/tree/main/Examples/)

