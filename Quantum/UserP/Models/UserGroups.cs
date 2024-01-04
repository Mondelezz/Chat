using Quantum.GroupFolder.Models;

namespace Quantum.UserP.Models
{
    public class UserGroups
    {
        public Group? Group { get; set; }
        public Guid GroupId { get; set; }

        public User? User { get; set; }
        public Guid UserId { get; set; }
    }
}
