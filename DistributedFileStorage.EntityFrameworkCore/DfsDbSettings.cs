using Newtonsoft.Json;

namespace DistributedFileStorage.EntityFrameworkCore
{
    public class DfsDbSettings
    {
        public JsonSerializerSettings JsonSerializer { get; set; } = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };
    }
}
