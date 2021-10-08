using Microsoft.EntityFrameworkCore;

namespace DistributedFileStorage.EntityFrameworkCore
{
    public delegate void DfsDbContextConfigurator(DbContextOptionsBuilder optionsBuilder);

    internal class DfsDbContext : DbContext
    {
        public DfsDbContext(DfsDbContextConfigurator configurator)
        {
            _configurator = configurator;

            DfsFileInfo = Set<DfsDbFileInfo>();
            DfsContentInfo = Set<DfsDbContentInfo>();
        }

        private readonly DfsDbContextConfigurator _configurator;

        public DbSet<DfsDbFileInfo> DfsFileInfo { get; private set; }
        public DbSet<DfsDbContentInfo> DfsContentInfo { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => _configurator(optionsBuilder);
    }

}
