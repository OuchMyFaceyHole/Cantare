using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class UserDateModel
    {
        [Key]
        public int EntryId { get; set; }

        public List<GuessModel> Guesses { get; set; } = new List<GuessModel>();

        public SongInfoModel Song { get; private set; }

        public DateTime SongDate => Song.DateUsed;

        public UserDateModel(SongInfoModel song)
        {
            Song = song;
        }

        private UserDateModel()
        {
            // Empty for EF navigation mapping
        }
    }
}
