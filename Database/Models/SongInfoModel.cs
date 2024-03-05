using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class SongInfoModel : SongModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public int StartPoint { get; private set; } = 0;

        public DateTime DateUsed { get; private set; } = DateTime.MinValue;

        public SongInfoModel() { }

        public SongInfoModel(string artist, string albumTitle, string songTitle, int startPoint) : base(artist, albumTitle, songTitle)
        {
            StartPoint = startPoint;
            DateUsed = DateTime.Now;
        }
    }
}
