using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DistributedFileStorage.MongoDB
{
    internal class DfsDbFileInfo<TMetadata>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Length { get; set; }
        public string? Metadata { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object obj) => Id == (obj as DfsDbFileInfo<TMetadata>)?.Id;
    }
}
