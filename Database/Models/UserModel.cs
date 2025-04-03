using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Cantare.Database.Models
{
    public class UserModel : IdentityUser
    {
        [Key]
        public int DatabaseKey { get; set; }

        public List<UserDateModel> CalendarAnswers { get; private set; } = new List<UserDateModel>();
    }
}
