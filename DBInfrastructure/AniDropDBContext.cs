using AniDrop.Domain;
using Microsoft.EntityFrameworkCore;

namespace AniDrop.DBInfrastructure;

public class AniDropDBContext: DbContext
{
    public AniDropDBContext(DbContextOptions<AniDropDBContext> options) : base(options)
    {
    }

    public DbSet<User> Users {  get; set; }
    public DbSet<ShikimoriProfile> ShikimoriProfiles {  get; set; }
    public DbSet<TitlePool> TitlePools { get; set; }
    public DbSet<TierList> TierLists { get; set; }
    public DbSet<AnimeTitle> AnimeTitles { get; set; }
    public DbSet<PoolItem> PoolItems { get; set; }
    public DbSet<RollHistory> RollHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
