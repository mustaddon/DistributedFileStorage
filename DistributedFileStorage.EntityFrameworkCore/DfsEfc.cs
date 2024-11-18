using System;

namespace DistributedFileStorage.EntityFrameworkCore
{
    internal class DfsEfc<TMetadata>(DfsDatabase<TMetadata> database, DfsSettings? settings)
        : DistributedFileStorage<TMetadata>(database, settings),
        IDisposable
    {
        public void Dispose()
        {
            database.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
