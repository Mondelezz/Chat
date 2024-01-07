using Quantum.UserP.Models;

namespace Quantum.GroupFolder.Models
{
    public class GroupRequest
    {
        public ICollection<UserInfoOutput> Users { get; set; } = new List<UserInfoOutput>();
    }
}
