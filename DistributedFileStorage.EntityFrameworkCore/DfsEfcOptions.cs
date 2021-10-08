namespace DistributedFileStorage.EntityFrameworkCore
{
    public class DfsEfcOptions
    {
        public DfsSettings FileStorage { get; } = new();
        public DfsDbSettings Database { get; } = new();
    }
}
