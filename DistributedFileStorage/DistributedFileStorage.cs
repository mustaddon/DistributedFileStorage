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

        public async Task<string> Add(IAsyncEnumerator<byte[]> content, string name, TMetadata? metadata, CancellationToken cancellationToken = default)
        {
            var id = _settings.IdGenerator();
            var path = _settings.PathGenerator(id);

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

            return id;
        }

        public Task UpdateInfo(string id, string name, TMetadata? metadata, CancellationToken cancellationToken = default)
        {
            return _database.Update(id, name, metadata, cancellationToken);
        }

        public async Task<DfsFileInfo<TMetadata>> GetInfo(string id, CancellationToken cancellationToken = default)
        {
            var item = await _database.Get(id, cancellationToken);
            return new(id, item.Name, item.Length, item.Metadata);
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
            var item = await _database.Get(id, cancellationToken);
            var buffer = new byte[4096];

            using var file = new FileStream(item.Path,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: buffer.Length, useAsync: true);

            var count = 0;
            while ((count = await file.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                yield return buffer.Length == count ? buffer : buffer.Take(count).ToArray();
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
            var tmpPath = item.Path;

            try
            {
                var storedPath = await _database.GetPath(item.Hash, cancellationToken);

                if (storedPath != null)
                {
                    item.Path = storedPath;

                    if (File.Exists(storedPath))
                        File.Delete(tmpPath);
                    else // heal missing path
                        File.Move(tmpPath, storedPath);
                }

                await _database.Add(item, cancellationToken);
            }
            catch
            {
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);

                throw;
            }
        }

        private static string GetHashString(byte[] hash, long length)
        {
            var bytes = hash.Concat(BitConverter.GetBytes(length)).ToArray();
            var zeroes = Enumerable.Range(0, bytes.Length).FirstOrDefault(i => bytes[bytes.Length - i - 1] > 0);
            return Convert.ToBase64String(bytes, 0, bytes.Length - zeroes);
        }

    }
}
