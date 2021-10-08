using DistributedFileStorage.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage
{
    public class DistributedFileStorage<TMetadata> : IDistributedFileStorage<TMetadata>
    {
        public DistributedFileStorage(IDfsDatabase<TMetadata> database, DfsSettings? settings = null)
        {
            _database = database;
            _settings = settings ?? new();
        }

        private readonly IDfsDatabase<TMetadata> _database;
        private readonly DfsSettings _settings;

        public async Task<string> Add(IAsyncEnumerator<byte[]> content, string name, TMetadata? metadata = default, CancellationToken cancellationToken = default)
        {
            var id = _settings.IdGenerator();
            var path = _settings.PathBuilder(id);

            try
            {
                var (hash, length) = await SaveContent(content, path, cancellationToken);

                await SaveInfo(new()
                {
                    Id = id,
                    Name = name,
                    Metadata = metadata,
                    Path = path,
                    Length = length,
                    Hash = GetHashString(hash, length),
                }, cancellationToken);
            }
            catch
            {
                if (File.Exists(path))
                    File.Delete(path);

                throw;
            }

            return id;
        }

        public Task Update(string id, string name, TMetadata? metadata = default, CancellationToken cancellationToken = default)
        {
            return _database.Update(id, name, metadata, cancellationToken);
        }

        public async IAsyncEnumerable<IDfsFileInfo<TMetadata>> GetInfos(IAsyncEnumerator<string> ids, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var batch in Batches(ids))
                foreach (var item in await _database.Get(batch, cancellationToken))
                    yield return Map(item);
        }

        public async Task Delete(string id, CancellationToken cancellationToken = default)
        {
            var item = await _database.Delete(id, cancellationToken);
            var free = item.Path != await _database.GetPath(item.Hash, cancellationToken);

            if (free && File.Exists(item.Path))
                File.Delete(item.Path);
        }

        public async IAsyncEnumerable<byte[]> GetContent(string id, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var item = (await _database.Get(new[] { id }, cancellationToken)).FirstOrDefault() ?? throw new FileNotFoundException();
            var buffer = new byte[4096];

            using var file = new FileStream(item.Path,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: buffer.Length, useAsync: true);

            var count = 0;
            while ((count = await file.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                var chunk = new byte[count];
                Array.Copy(buffer, 0, chunk, 0, count);
                yield return chunk;
            }
        }



        private async Task<(byte[] hash, long length)> SaveContent(IAsyncEnumerator<byte[]> content, string path, CancellationToken cancellationToken)
        {
            (new FileInfo(path)).Directory.Create();

            using var file = File.Create(path);
            using var sha = SHA256.Create();

            sha.Initialize();
            var length = 0L;

            while (await content.MoveNextAsync())
            {
                sha.TransformBlock(content.Current, 0, content.Current.Length, null, 0);
                await file.WriteAsync(content.Current, 0, content.Current.Length, cancellationToken);
                length += content.Current.Length;
            }

            sha.TransformFinalBlock(new byte[0], 0, 0);
            return (sha.Hash, length);
        }

        private async Task SaveInfo(DfsDbItem<TMetadata> item, CancellationToken cancellationToken)
        {
            var storedPath = await _database.GetPath(item.Hash, cancellationToken);

            if (storedPath != null)
            {
                if (File.Exists(storedPath))
                    File.Delete(item.Path);
                else // heal missing path
                    File.Move(item.Path, storedPath);

                item.Path = storedPath;
            }

            await _database.Add(item, cancellationToken);
        }

        private static string GetHashString(byte[] hash, long length)
        {
            var bytes = hash.Concat(BitConverter.GetBytes(length)).ToArray();
            var zeroes = Enumerable.Range(0, bytes.Length).FirstOrDefault(i => bytes[bytes.Length - i - 1] > 0);
            return Convert.ToBase64String(bytes, 0, bytes.Length - zeroes);
        }

        private async IAsyncEnumerable<IEnumerable<T>> Batches<T>(IAsyncEnumerator<T> items, int size = 1000)
        {
            var tmp = new List<T>();

            while (await items.MoveNextAsync())
            {
                tmp.Add(items.Current);

                if (tmp.Count >= size)
                {
                    yield return tmp;
                    tmp.Clear();
                }
            }

            if (tmp.Any())
                yield return tmp;
        }

        private DfsFileInfo<TMetadata> Map(DfsDbItem<TMetadata> item)
        {
            return new DfsFileInfo<TMetadata>(item.Id, item.Name, item.Length, item.Metadata);
        }
    }
}
