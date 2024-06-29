using Microsoft.EntityFrameworkCore;
using Cantare.Database.Models;

namespace Cantare.Database
{
    public class BackendDatabase : DbContext
    {
        public BackendDatabase(DbContextOptions<BackendDatabase> options) : base(options) { }

        public DbSet<SongModel> AvailableSongs { get; set; }

        public DbSet<CalendarSongModel> SongCalender { get; set; }

        public DbSet<SongImageModel> SongImages { get; set; }   

        public DbSet<UserModel> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                .HasMany(u => u.CalendarAnswers)
                .WithOne()
                .IsRequired();

            modelBuilder.Entity<UserDateModel>()
                .HasMany(u => u.Guesses)
                .WithOne()
                .IsRequired();

            modelBuilder.Entity<CalendarSongModel>().Navigation(nav => nav.SongInfo).AutoInclude();

            base.OnModelCreating(modelBuilder);
        }
    }
}
