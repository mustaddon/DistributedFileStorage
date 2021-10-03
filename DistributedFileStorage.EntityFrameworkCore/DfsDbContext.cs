using Microsoft.EntityFrameworkCore;

namespace DistributedFileStorage.EntityFrameworkCore
{
    public delegate void DfsDbContextConfigurator(DbContextOptionsBuilder optionsBuilder);

    public class DfsDbContext : DbContext
    {
        public DfsDbContext(DfsDbContextConfigurator configurator)
        {
            _configurator = configurator;

            DfsFileInfo = Set<DfsDbFileInfo>();
            DfsContentInfo = Set<DfsDbContentInfo>();
        }

        private readonly DfsDbContextConfigurator _configurator;

        internal DbSet<DfsDbFileInfo> DfsFileInfo { get; private set; }
        internal DbSet<DfsDbContentInfo> DfsContentInfo { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => _configurator(optionsBuilder);
    }

}
