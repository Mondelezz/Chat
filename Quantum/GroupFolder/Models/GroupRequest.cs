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
        public ICollection<GroupRequestUserInfoOutput> Users { get; set; } = new List<GroupRequestUserInfoOutput>();
        
        public int CountRequests { get; set; }
    }
}
