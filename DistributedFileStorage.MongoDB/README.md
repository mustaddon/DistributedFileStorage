# DistributedFileStorage.MongoDB [![NuGet version](https://badge.fury.io/nu/DistributedFileStorage.MongoDB.svg)](http://badge.fury.io/nu/DistributedFileStorage.MongoDB)
DistributedFileStorage with MongoDB implementation of IDfsDatabase


## Features
* Storing metadata in the database (SQL/NoSQL)
* Storing files in the file system
* Deduplication of files by content
* Distributed storage (multiple disks)



## Example: Simple console app
```
dotnet new console --name "DfsExample"
cd DfsExample
dotnet add package DistributedFileStorage.MongoDB
dotnet add package Microsoft.Extensions.DependencyInjection
```

program.cs:
```C#
using DistributedFileStorage;
using Microsoft.Extensions.DependencyInjection;


// add services to the container
var services = new ServiceCollection()
    .AddDfsMongo<string>(options =>
    {
        // add database settings 
        options.Database.ConnectionString = "mongodb://localhost:27017";

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
