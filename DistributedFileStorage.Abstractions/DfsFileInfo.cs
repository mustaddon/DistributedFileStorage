namespace DistributedFileStorage.Abstractions
{
    public class DfsFileInfo<TMetadata> : IDfsFileInfo<TMetadata>
    {
        public DfsFileInfo(string id, string name, long length, TMetadata? metadata = default)
        {
            Id = id;
            Name = name;
            Length = length;
            Metadata = metadata;
        }

        public string Id { get; }
        public string Name { get; }
        public long Length { get; }
        public TMetadata? Metadata { get; }

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object obj) => Id == (obj as DfsFileInfo<TMetadata>)?.Id;
    }
}
