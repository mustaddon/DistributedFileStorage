dotnet build -c Release 
dotnet pack .\DistributedFileStorage\ -c Release -o ..\_publish
dotnet pack .\DistributedFileStorage.EntityFrameworkCore\ -c Release -o ..\_publish
dotnet pack .\DistributedFileStorage.MongoDB\ -c Release -o ..\_publish
