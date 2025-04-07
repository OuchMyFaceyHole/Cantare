using Cantare.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cantare.Database
{
    public class BackendDatabase : IdentityDbContext<UserModel>
    {
        public BackendDatabase(DbContextOptions<BackendDatabase> options) : base(options) { }

        public DbSet<SongModel> AvailableSongs { get; set; }

        public DbSet<CalendarSongModel> SongCalender { get; set; }

        public DbSet<SongImageModel> SongImages { get; set; }

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

            modelBuilder.Entity<GuessModel>()
                .HasOne(u => u.Song)
                .WithMany()
                .IsRequired();

            modelBuilder.Entity<CalendarSongModel>().Navigation(nav => nav.SongInfo).AutoInclude();

            base.OnModelCreating(modelBuilder);
        }
    }
}
