using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage
{
    public static class DfsExtensions
    {
        public static Task<string> Add<TMetadata>(this IDistributedFileStorage<TMetadata> dfs,
            Stream content, string name, TMetadata? metadata = default, CancellationToken cancellationToken = default)
        {
            return dfs.Add(GetAsyncEnumerator(content, cancellationToken), name, metadata, cancellationToken);
        }

        public static Task<string> Add<TMetadata>(this IDistributedFileStorage<TMetadata> dfs,
            byte[] content, string name, TMetadata? metadata = default, CancellationToken cancellationToken = default)
        {
            return dfs.Add(GetChunkEnumerator(content), name, metadata, cancellationToken);
        }

        public static Task<string> Add<TMetadata>(this IDistributedFileStorage<TMetadata> dfs,
            IAsyncEnumerable<byte[]> content, string name, TMetadata? metadata = default, CancellationToken cancellationToken = default)
        {
            return dfs.Add(content.GetAsyncEnumerator(cancellationToken), name, metadata, cancellationToken);
        }

        public static IAsyncEnumerable<IDfsFileInfo<TMetadata>> GetInfos<TMetadata>(this IReadOnlyDfs<TMetadata> dfs,
            IAsyncEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            return dfs.GetInfos(ids.GetAsyncEnumerator(cancellationToken), cancellationToken);
        }

        public static IAsyncEnumerable<IDfsFileInfo<TMetadata>> GetInfos<TMetadata>(this IReadOnlyDfs<TMetadata> dfs,
            IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            return dfs.GetInfos(GetAsyncEnumerator(ids), cancellationToken);
        }

        public static async Task<IDfsFileInfo<TMetadata>> GetInfo<TMetadata>(this IReadOnlyDfs<TMetadata> dfs,
            string id, CancellationToken cancellationToken = default)
        {
            var enumerator = dfs.GetInfos(GetAsyncEnumerator(new[] { id }), cancellationToken).GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
                throw new FileNotFoundException();

            return enumerator.Current;
        }



        private static async IAsyncEnumerator<byte[]> GetAsyncEnumerator(Stream stream, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[4096];
            int count;

            while ((count = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                if (count == buffer.Length)
                {
                    yield return buffer;
                    continue;
                }

                var chunk = new byte[count];
                Array.Copy(buffer, 0, chunk, 0, count);
                yield return chunk;
            }
        }

        private static async IAsyncEnumerator<byte[]> GetChunkEnumerator(byte[] data)
        {
            var bufferSize = 4096;

            if (data.Length <= bufferSize)
            {
                yield return await Task.FromResult(data);
            }
            else
            {
                var buffer = new byte[bufferSize];
                for (var i = 0; i < data.Length; i += buffer.Length)
                {
                    var count = Math.Min(buffer.Length, data.Length - i);
                    var chunk = count == buffer.Length ? buffer : new byte[count];
                    Array.Copy(data, i, chunk, 0, chunk.Length);
                    yield return chunk;
                }
            }
        }

        private static async IAsyncEnumerator<T> GetAsyncEnumerator<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
                yield return await Task.FromResult(item);
        }
    }
}
