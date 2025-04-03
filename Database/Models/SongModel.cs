using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cantare.Database.Models
{
    public class SongModel
    {
        [JsonIgnore]
        [Key]
        public int DatabaseKey { get; set; }

        public string Artist { get; set; } = "";

        public string AlbumTitle { get; set; } = "";

        public string SongTitle { get; set; } = "";

        [JsonIgnore]
        public string SongPath { get; set; } = "";

        [JsonIgnore]
        public int SongLength { get; set; } = int.MinValue;

        [JsonIgnore]
        public int AudioStreamIndex { get; set; } = int.MinValue;

        [JsonIgnore]
        public SongImageModel SongImage { get; set; }

        [JsonIgnore]
        public bool IsEnabled { get; set; } = true;

        public SongModel() { }

        public SongModel(string artist, string albumTitle, string songTitle, string songPath, int songLength, int audioStreamIndex, SongImageModel songImage)
        {
            Artist = artist;
            AlbumTitle = albumTitle;
            SongTitle = songTitle;
            SongPath = songPath;
            SongLength = songLength;
            AudioStreamIndex = audioStreamIndex;
            SongImage = songImage;
        }
    }
}
