using System;
using System.IO;

namespace DistributedFileStorage
{
    public class DfsSettings
    {
        public Func<string> IdGenerator { get; set; }
            = static () => Guid.NewGuid().ToString("n");

        public Func<string, string> PathBuilder { get; set; }
            = static (fileId) => Path.GetFullPath($@"_dfs\{DateTime.Now:yyyy\\MM\\dd}\{fileId}");
    }
}
