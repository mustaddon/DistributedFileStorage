using System;
using System.IO;

namespace DistributedFileStorage
{
    public class DfsSettings
    {
        public Func<string> IdGenerator { get; set; }
            = static () => Guid.NewGuid().ToString("n");

        public Func<string, string> PathGenerator { get; set; }
            = static (id) => Path.GetFullPath(Path.Combine("stored_content", DateTime.Now.ToString(@"yyyy\\MM\\dd"), id));
    }
}
