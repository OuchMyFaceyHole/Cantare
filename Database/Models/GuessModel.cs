using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class GuessModel : SongModel
    {
        [Key]
        public int GuessId { get; private set; }

        public GuessEnumeration GuessStatus { get; private set; }

        public int GuessNumber { get; private set; }

        public GuessModel(
            GuessEnumeration guessStatus, 
            int guessNumber, 
            string artist, 
            string albumTitle,
            string songTitle) 
            : base(artist, albumTitle, songTitle)
        {
            GuessStatus = guessStatus;
            GuessNumber = guessNumber;
        }
    }
}
