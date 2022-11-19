using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage.EntityFrameworkCore
{
    public class DfsDatabase<TMetadata> : IDfsDatabase<TMetadata>
    {
        public DfsDatabase(DfsDbSettings? settings = null)
        {
            _settings = settings ?? new();
            _context = new(_settings.ContextConfigurator);
        }

        readonly DfsDbContext _context;
        readonly DfsDbSettings _settings;

        public Task Add(DfsDbItem<TMetadata> item, CancellationToken cancellationToken = default)
        {
            var fileInfo = new DfsDbFileInfo
            {
                Id = item.Id,
                Name = item.Name,
                Metadata = SerializeMetadata(item.Metadata),
                ContentId = item.Hash,
            };

            var contentInfo = new DfsDbContentInfo
            {
                Id = item.Hash,
                Length = item.Length,
                Path = item.Path,
            };

            return WithRetry(1, async (i) =>
            {
                _context.Entry(contentInfo).State =
                    !await _context.DfsContentInfo.AnyAsync(x => x.Id == item.Hash, cancellationToken)
                        ? EntityState.Added : EntityState.Detached;

                _context.Entry(fileInfo).State = EntityState.Added;

                await _context.SaveChangesAsync(cancellationToken);
            });
        }

        public async Task<IEnumerable<DfsDbItem<TMetadata>>> Get(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            var idsArray = (ids as string[]) ?? ids.ToArray();
            return (await _context.DfsFileInfo.AsNoTracking()
                .Include(x => x.Content)
                .Where(x => idsArray.Contains(x.Id))
                .Take(idsArray.Length)
                .ToListAsync(cancellationToken))
                .Select(Map);
        }


        public async Task<string?> GetPath(string hash, CancellationToken cancellationToken = default)
        {
            return (await _context.DfsContentInfo.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == hash, cancellationToken))?.Path;
        }

        public async Task Update(string id, string name, TMetadata? metadata, CancellationToken cancellationToken = default)
        {
            var entity = await _context.DfsFileInfo.SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new FileNotFoundException();
            entity.Name = name;
            entity.Metadata = SerializeMetadata(metadata);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DfsDbItem<TMetadata>> Delete(string id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.DfsFileInfo.Include(x => x.Content)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new FileNotFoundException();

            _context.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            if (!await _context.DfsFileInfo.AnyAsync(x => x.ContentId == entity.ContentId, cancellationToken))
            {
                _context.Remove(entity.Content!);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Map(entity);
        }

        private DfsDbItem<TMetadata> Map(DfsDbFileInfo entity)
        {
            return new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Hash = entity.ContentId,
                Length = entity.Content?.Length ?? 0,
                Path = entity.Content?.Path ?? string.Empty,
                Metadata = DeserializeMetadata(entity.Metadata),
            };
        }

        private static async Task WithRetry(int retries, Func<int, Task> task)
        {
            for (var i = 0; i <= retries; i++)
                try
                {
                    await task(i); return;
                }
                catch
                {
                    if (i == retries) throw;
                }
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
