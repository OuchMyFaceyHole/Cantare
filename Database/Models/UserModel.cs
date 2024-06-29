using System.ComponentModel.DataAnnotations;

namespace Cantare.Database.Models
{
    public class UserModel
    {
        [Key]
        public int DatabaseKey { get; set; }

        public List<UserDateModel> CalendarAnswers { get; private set; } = new List<UserDateModel>();
    }
}
