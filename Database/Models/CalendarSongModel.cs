using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class CalendarSongModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public SongModel SongInfo { get; set; } = new SongModel();

        public int StartPoint { get; private set; } = 0;

        public DateTime DateUsed { get; private set; } = DateTime.MinValue;

        public CalendarSongModel() { }

        public CalendarSongModel(int startPoint, SongModel songInfo)
        {
            StartPoint = startPoint;
            DateUsed = DateTime.Now.Date;
            SongInfo = songInfo;
        }
    }
}
