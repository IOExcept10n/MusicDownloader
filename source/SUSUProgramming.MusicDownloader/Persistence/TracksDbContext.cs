using Microsoft.EntityFrameworkCore;

namespace SUSUProgramming.MusicDownloader.Persistence
{
    public class TracksDbContext : DbContext
    {
        public DbSet<TrackStatsModel> TrackStats => Set<TrackStatsModel>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data source=tracks.db");
        }
    }
}
