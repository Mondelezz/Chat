using Quantum.UserP.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Quantum.GroupFolder.Models
{
    public class GroupRequest
    {
        [Key]
        public Guid GroupRequestId { get; set; }
        public Group Group { get; set; }
        public Guid GroupId { get; set; }
        [JsonIgnore]
        public ICollection<UserInfoOutput> Users { get; set; } = new List<UserInfoOutput>();
        public int CountRequests { get; set; }
    }
}
