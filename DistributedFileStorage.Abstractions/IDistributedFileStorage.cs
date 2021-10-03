using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage.Abstractions
{
    public interface IDistributedFileStorage<TMetadata>
    {
        Task<string> Add(IAsyncEnumerator<byte[]> content, string name, TMetadata? metadata, CancellationToken cancellationToken = default);
        IAsyncEnumerable<byte[]> GetContent(string id, CancellationToken cancellationToken = default);
        Task<DfsFileInfo<TMetadata>> GetInfo(string id, CancellationToken cancellationToken = default);
        Task UpdateInfo(string id, string name, TMetadata? metadata, CancellationToken cancellationToken = default);
        Task Delete(string id, CancellationToken cancellationToken = default);
    }
}