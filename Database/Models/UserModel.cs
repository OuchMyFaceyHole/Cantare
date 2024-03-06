using System.ComponentModel.DataAnnotations;

namespace SwiftTrueRandom.Database.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        public List<UserDateModel> CalendarAnswers { get; private set; } = new List<UserDateModel>();
    }
}
