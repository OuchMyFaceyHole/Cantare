using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class SongModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public string Artist { get; private set; } = "";

        public string AlbumTitle { get; private set; } = "";

        public string SongTitle { get; private set; } = "";

        public string SongPath { get; private set; } = "";

        public SongModel() { }

        public SongModel(string artist, string albumTitle, string songTitle, string songPath)
        {
            Artist = artist;
            AlbumTitle = albumTitle;
            SongTitle = songTitle;
            SongPath = songPath;
        }
    }
}
