using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class UserDateModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public int UserId { get; set; }

        public List<GuessModel> Guesses { get; set; } = new List<GuessModel>();

        public CalendarSongModel Song { get; private set; }

        public DateTime SongDate => Song.DateUsed;

        public UserDateModel() { }

        public UserDateModel(CalendarSongModel song)
        {
            Song = song;
        }
    }
}
