using Quantum.UserP.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quantum.GroupFolder.Models
{
    public class GroupRequest
    {
        [Key]
        [ForeignKey("Group")]
        public Guid GroupId { get; set; }
        public Group Group { get; set; }      
        public ICollection<UserInfoOutput> Users { get; set; } = new List<UserInfoOutput>();
        public int CountRequsts { get; set; }
    }
}
