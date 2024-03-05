using Microsoft.EntityFrameworkCore;
using SwiftTrueRandom.Database.Models;

namespace SwiftTrueRandom.Database
{
    public class BackendDatabase : DbContext
    {
        public BackendDatabase(DbContextOptions<BackendDatabase> options) : base(options) { }

        public DbSet<SongInfoModel> SongCalender { get; set; }
    }
}
