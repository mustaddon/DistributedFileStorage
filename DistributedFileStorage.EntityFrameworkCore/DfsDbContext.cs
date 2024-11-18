using Microsoft.EntityFrameworkCore;

namespace DistributedFileStorage.EntityFrameworkCore
{
    public delegate void DfsDbContextConfigurator(DbContextOptionsBuilder optionsBuilder);

    internal class DfsDbContext : DbContext
    {
        public DfsDbContext(DfsDbSettings settings)
        {
            _settings = settings;

            DfsFileInfo = Set<DfsDbFileInfo>();
            DfsContentInfo = Set<DfsDbContentInfo>();
        }

        private readonly DfsDbSettings _settings;

        public DbSet<DfsDbFileInfo> DfsFileInfo { get; private set; }
        public DbSet<DfsDbContentInfo> DfsContentInfo { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => _settings.ContextConfigurator(optionsBuilder);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DfsDbFileInfo>().ToTable(_settings.FileInfoTable);
            modelBuilder.Entity<DfsDbContentInfo>().ToTable(_settings.ContentInfoTable);

            base.OnModelCreating(modelBuilder);
        }
    }

}
