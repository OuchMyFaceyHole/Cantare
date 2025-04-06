using System.ComponentModel.DataAnnotations;

namespace Cantare.Database.Models
{
    public class GuessModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public GuessEnumeration GuessStatus { get; private set; }

        public SongModel Song { get; set; }

        public GuessModel() { }

        public GuessModel(GuessEnumeration guessStatus, SongModel song)
        {
            GuessStatus = guessStatus;
            Song = song;
        }
    }
}
