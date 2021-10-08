using Newtonsoft.Json;
using System;

namespace DistributedFileStorage.EntityFrameworkCore
{
    public class DfsDbSettings
    {
        public int? MaxMetadataLength { get; set; }

        public JsonSerializerSettings JsonSerializer { get; set; } = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public DfsDbContextConfigurator ContextConfigurator { get; set; } = static x =>
        {
            throw new InvalidOperationException($"Database provider not configured. A provider can be configured by setting the '{nameof(DfsDbSettings)}.{nameof(ContextConfigurator)}' property.");
        };
    }
}
