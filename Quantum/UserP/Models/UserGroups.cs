using Quantum.GroupFolder.Models;
using System.Text.Json.Serialization;

namespace Quantum.UserP.Models
{
    public class UserGroups
    {
        [JsonIgnore]
        public Group? Group { get; set; }
        public Guid GroupId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
        public Guid UserId { get; set; }
    }
}
