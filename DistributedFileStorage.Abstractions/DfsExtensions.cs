using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage.Abstractions
{
    public static class DfsExtensions
    {
        public static Task<string> Add<TMetadata>(this IDistributedFileStorage<TMetadata> dfs, 
            Stream stream, string name, TMetadata? metadata, CancellationToken cancellationToken = default)
        {
            return dfs.Add(stream.GetEnumerator(), name, metadata, cancellationToken);
        }

        public static Task<string> Add<TMetadata>(this IDistributedFileStorage<TMetadata> dfs,
            IAsyncEnumerable<byte[]> content, string name, TMetadata? metadata, CancellationToken cancellationToken = default)
        {
            return dfs.Add(content.GetAsyncEnumerator(cancellationToken), name, metadata, cancellationToken);
        }

        public static async IAsyncEnumerator<byte[]> GetEnumerator(this Stream stream, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[4096];
            int readCount;

            while ((readCount = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                yield return readCount < buffer.Length ? buffer.Take(readCount).ToArray() : buffer;
        }
    }
}
