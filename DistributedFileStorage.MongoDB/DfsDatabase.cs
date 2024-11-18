using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage.MongoDB
{
    public class DfsDatabase<TMetadata> : IDfsDatabase<TMetadata>, IDisposable
    {
        public DfsDatabase(DfsDbSettings? settings = null)
        {
            _settings = settings ?? new();

            _fileInfos = new(() =>
            {
                var client = new MongoClient(_settings.ConnectionString);
                var database = client.GetDatabase(_settings.DatabaseName, _settings.DatabaseSettings);
                return database.GetCollection<DfsDbFileInfo<TMetadata>>(_settings.CollectionName, _settings.CollectionSettings);
            });
        }

        readonly DfsDbSettings _settings;
        readonly Lazy<IMongoCollection<DfsDbFileInfo<TMetadata>>> _fileInfos;

        public void Dispose()
        {
            if (_fileInfos.IsValueCreated)
                _fileInfos.Value.Database.Client.Dispose();

            GC.SuppressFinalize(this);
        }

        public Task Add(DfsDbItem<TMetadata> item, CancellationToken cancellationToken = default)
        {
            return _fileInfos.Value.InsertOneAsync(new()
            {
                Id = item.Id,
                Name = item.Name,
                Hash = item.Hash,
                Length = item.Length,
                Path = item.Path,
                Metadata = SerializeMetadata(item.Metadata),
            }, null, cancellationToken);
        }

        public async Task<IEnumerable<DfsDbItem<TMetadata>>> Get(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            var idsArray = (ids as string[]) ?? ids.ToArray();
            return (await _fileInfos.Value
                .Find(x => idsArray.Contains(x.Id))
                .ToListAsync(cancellationToken))
                .Select(Map);
        }

        public async Task<string?> GetPath(string hash, CancellationToken cancellationToken = default)
        {
            return (await _fileInfos.Value.Find(x => x.Hash == hash)
                .FirstOrDefaultAsync(cancellationToken))?.Path;
        }

        public async Task Update(string id, string name, TMetadata? metadata, CancellationToken cancellationToken = default)
        {
            var update = Builders<DfsDbFileInfo<TMetadata>>.Update
                .Set(x => x.Name, name)
                .Set(x => x.Metadata, SerializeMetadata(metadata));

            var result = await _fileInfos.Value.UpdateOneAsync(x => x.Id == id, update, null, cancellationToken);
            if (result.MatchedCount == 0) throw new FileNotFoundException();
        }

        public async Task<DfsDbItem<TMetadata>> Delete(string id, CancellationToken cancellationToken = default)
        {
            var entity = await _fileInfos.Value.FindOneAndDeleteAsync(x => x.Id == id, null, cancellationToken) ?? throw new FileNotFoundException();
            return Map(entity);
        }

        private DfsDbItem<TMetadata> Map(DfsDbFileInfo<TMetadata> entity)
        {
            return new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Hash = entity.Hash,
                Length = entity.Length,
                Path = entity.Path,
                Metadata = DeserializeMetadata(entity.Metadata),
            };
        }

        private string? SerializeMetadata(TMetadata? metadata)
        {
            var result = metadata == null || metadata is string ? metadata as string
                : JsonConvert.SerializeObject(metadata, _settings.JsonSerializer);

            if (result?.Length > _settings.MaxMetadataLength)
                throw new InvalidOperationException($"Metadata length limit exceeded ({result?.Length}/{_settings.MaxMetadataLength})");

            return result;
        }

        private TMetadata? DeserializeMetadata(string? metadata)
        {
            if (string.IsNullOrEmpty(metadata))
                return default;

            if (typeof(TMetadata) == typeof(string))
                return (TMetadata?)(metadata as object);

            try
            {
                return JsonConvert.DeserializeObject<TMetadata?>(metadata, _settings.JsonSerializer);
            }
            catch
            {
                return default;
            }
        }
    }
}
