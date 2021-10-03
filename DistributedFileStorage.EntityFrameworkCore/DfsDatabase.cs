using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedFileStorage.EntityFrameworkCore
{
    public class DfsDatabase<TMetadata> : IDfsDatabase<TMetadata>
    {
        public DfsDatabase(DfsDbContext context, DfsDbSettings? settings = null)
        {
            _context = context;
            _settings = settings ?? new();
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

        public async Task<DfsDbItem<TMetadata>> Get(string id, CancellationToken cancellationToken = default)
        {
            return Map(await _context.DfsFileInfo.AsNoTracking()
                .Include(x => x.Content)
                .SingleAsync(x => x.Id == id, cancellationToken));
        }


        public async Task<string?> GetPath(string hash, CancellationToken cancellationToken = default)
        {
            return (await _context.DfsContentInfo.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == hash, cancellationToken))?.Path;
        }

        public async Task Update(string id, string name, TMetadata? metadata, CancellationToken cancellationToken = default)
        {
            var entity = await _context.DfsFileInfo.SingleAsync(x => x.Id == id, cancellationToken);
            entity.Name = name;
            entity.Metadata = SerializeMetadata(metadata);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<DfsDbItem<TMetadata>> Delete(string id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.DfsFileInfo.Include(x => x.Content).SingleAsync(x => x.Id == id, cancellationToken);

            _context.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            if (!await _context.DfsFileInfo.AnyAsync(x => x.ContentId == entity.ContentId, cancellationToken))
            {
                _context.Remove(entity.Content);
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

        private async Task WithRetry(int retries, Func<int, Task> task)
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
            if (metadata == null || metadata is string)
                return metadata as string;

            return JsonConvert.SerializeObject(metadata, _settings.JsonSerializer);
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
