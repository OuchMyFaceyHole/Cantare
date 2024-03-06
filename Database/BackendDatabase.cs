using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SwiftTrueRandom.Database.Models;

namespace SwiftTrueRandom.Database
{
    public class BackendDatabase : DbContext
    {
        public BackendDatabase(DbContextOptions<BackendDatabase> options) : base(options)
        {
            //this.Database.EnsureCreated();
        }

        public DbSet<SongInfoModel> SongCalender { get; set; }

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


            base.OnModelCreating(modelBuilder);
        }
    }
}
