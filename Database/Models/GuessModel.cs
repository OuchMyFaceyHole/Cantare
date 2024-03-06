using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class GuessModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public GuessEnumeration GuessStatus { get; private set; }

        public int GuessNumber { get; private set; }

        public SongModel Song { get; set; }

        public GuessModel() { }

        public GuessModel(GuessEnumeration guessStatus, int guessNumber, SongModel song)
        {
            GuessStatus = guessStatus;
            GuessNumber = guessNumber;
            Song = song;
        }
    }
}
