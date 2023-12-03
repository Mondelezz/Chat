using System.ComponentModel.DataAnnotations;

namespace Quantum.Models
{
    public class Friends
    {
        [Key]
        public int Id { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public Guid UserId { get; set; }
        public Guid FriendId { get; set; }
    }
}
