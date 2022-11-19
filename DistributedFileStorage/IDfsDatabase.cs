using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage
{
    public interface IDfsDatabase<TMetadata>
    {
        Task Add(DfsDbItem<TMetadata> item, CancellationToken cancellationToken = default);
        Task Update(string id, string name, TMetadata? metadata, CancellationToken cancellationToken = default);
        Task<DfsDbItem<TMetadata>> Delete(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<DfsDbItem<TMetadata>>> Get(IEnumerable<string> ids, CancellationToken cancellationToken = default);
        Task<string?> GetPath(string hash, CancellationToken cancellationToken = default);
    }

    public class DfsDbItem<TMetadata>
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public long Length { get; set; }
        public TMetadata? Metadata { get; set; }

        public override int GetHashCode() => Id.GetHashCode();
        public override bool Equals(object? obj) => Id == (obj as DfsDbItem<TMetadata>)?.Id;
    }
}
