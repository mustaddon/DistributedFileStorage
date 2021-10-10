using System;

namespace DistributedFileStorage.MongoDB
{
    public class DfsMongoOptions
    {
        public DfsDbSettings Database { get; } = new();

        public DfsSettings FileStorage { get; } = new()
        {
            IdGenerator = static () =>
            {
                var guid = Guid.NewGuid().ToString("n");
                return string.Concat(guid.Substring(0, 12), guid.Substring(guid.Length - 12, 12));
            },
        };
    }
}
