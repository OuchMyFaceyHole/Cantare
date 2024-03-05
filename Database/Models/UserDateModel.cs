using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class UserDateModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public int UserId { get; set; }

        public List<GuessModel> Guesses { get; set; } = new List<GuessModel>();

        public SongInfoModel Song { get; private set; }

        public DateTime SongDate { get => Song.DateUsed;  }

        public UserDateModel() { }

        public UserDateModel(SongInfoModel song)
        {
            Song = song;
        }
    }
}
