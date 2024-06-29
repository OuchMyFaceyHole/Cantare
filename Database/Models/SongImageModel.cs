namespace Cantare.Database.Models
{
    public class SongImageModel
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int DatabaseKey { get; set; }
        public byte[] ImageData { get; set; }

        public SongImageModel() { } 

        public SongImageModel(byte[] imageData)
        {
            ImageData = imageData;
        }
    }
}
