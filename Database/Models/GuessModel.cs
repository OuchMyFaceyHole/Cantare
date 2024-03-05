namespace SwiftTrueRandom.Database.Models
{
    public class GuessModel : SongModel
    {
        public GuessEnumeration GuessStatus { get; private set; }

        public int GuessNumber { get; private set; }

        public GuessModel() { }

        public GuessModel(GuessEnumeration guessStatus, int guessNumber, 
            string artist, string albumTitle, string songTitle) : base(artist, albumTitle, songTitle)
        {
            GuessStatus = guessStatus;
            GuessNumber = guessNumber;
        }
    }
}
