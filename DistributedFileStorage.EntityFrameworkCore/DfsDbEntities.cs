namespace DistributedFileStorage.EntityFrameworkCore
{
    internal class DfsDbFileInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Metadata { get; set; }
        public string ContentId { get; set; } = string.Empty;
        public DfsDbContentInfo? Content { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object obj) => Id == (obj as DfsDbFileInfo)?.Id;
    }

    internal class DfsDbContentInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Length { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object obj) => Id == (obj as DfsDbContentInfo)?.Id;
    }
}
