using System;

namespace DistributedFileStorage.MongoDB
{
    internal class DfsMongo<TMetadata>(DfsDatabase<TMetadata> database, DfsSettings? settings)
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
