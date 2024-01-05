using Quantum.GroupFolder.Enums;

namespace Quantum.GroupFolder.Models
{
    public class GroupUserRole
    {
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public RolesGroupType Role { get; set; }
    }
}