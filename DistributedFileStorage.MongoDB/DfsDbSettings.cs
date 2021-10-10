using MongoDB.Driver;
using Newtonsoft.Json;

namespace DistributedFileStorage.MongoDB
{
    public class DfsDbSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "dfs";
        public MongoDatabaseSettings? DatabaseSettings { get; set; }
        public string CollectionName { get; set; } = "fileinfos";
        public MongoCollectionSettings? CollectionSettings { get; set; }

        public int? MaxMetadataLength { get; set; }

        public JsonSerializerSettings JsonSerializer { get; set; } = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };
    }
}
