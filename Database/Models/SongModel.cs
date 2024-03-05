namespace SwiftTrueRandom.Database.Models
{
    public class SongModel
    {
        public string Artist { get; private set; } = "";

        public string AlbumTitle { get; private set; } = "";

        public string SongTitle { get; private set; } = "";

        public SongModel() { }

        public SongModel(string artist, string albumTitle, string songTitle)
        {
            Artist = artist;
            AlbumTitle = albumTitle;
            SongTitle = songTitle;
        }
    }
}
