using Quantum.UserP.Models;

namespace Quantum.GroupFolder.Models
{
    public class GroupRequestUserInfoOutput
    {
        public GroupRequest? GroupRequest { get; set; }
        public Guid GroupRequestId { get; set; }

        public UserInfoOutput? UserInfoOutput { get; set; }
        public Guid UserInfoOutputId { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
