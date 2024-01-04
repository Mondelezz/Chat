using Quantum.UserP.Models;
using StackExchange.Redis;

namespace Quantum.GroupFolder.Models
{
    public class Group
    {
        public string NameGroup { get; set; } = string.Empty;
        public string? DescriptionGroup { get; set; } = string.Empty;
        public Guid GroupId { get; set; }
        public List<UserInfoOutput> Users { get; set; } = new List<UserInfoOutput>();
        public bool StatusAccess { get; set; } = false;
        public int CountMembers { get; set; } 
        public string LinkInvitation { get; set; } = string.Empty;
    }
}
