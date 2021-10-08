using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage.Abstractions
{
    public interface IDistributedFileStorage<TMetadata> : IReadOnlyDfs<TMetadata>
    {
        Task<string> Add(IAsyncEnumerator<byte[]> content, string name, TMetadata? metadata = default, CancellationToken cancellationToken = default);
        Task Update(string id, string name, TMetadata? metadata = default, CancellationToken cancellationToken = default);
        Task Delete(string id, CancellationToken cancellationToken = default);
    }

    public interface IReadOnlyDfs<out TMetadata>
    {
        IAsyncEnumerable<byte[]> GetContent(string id, CancellationToken cancellationToken = default);
        IAsyncEnumerable<IDfsFileInfo<TMetadata>> GetInfos(IAsyncEnumerator<string> ids, CancellationToken cancellationToken = default);
    }

    public interface IDfsFileInfo<out TMetadata>
    {
        string Id { get; }
        string Name { get; }
        long Length { get; }
        TMetadata? Metadata { get; }
    }
}