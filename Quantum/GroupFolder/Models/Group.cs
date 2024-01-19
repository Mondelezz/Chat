using Quantum.UserP.Models;
using System.Text.Json.Serialization;

namespace Quantum.GroupFolder.Models
{
    public class Group
    {
        // Id
        public Guid GroupId { get; set; }
        // Название
        public string NameGroup { get; set; } = string.Empty;
        // Описание
        public string? DescriptionGroup { get; set; } = string.Empty;       
        // Участники
        [JsonIgnore]
        public ICollection<UserGroups> Members { get; set; } = new List<UserGroups>();
        // Заявки
        [JsonIgnore]
        public GroupRequest GroupRequest { get; set; } = new GroupRequest();
        public Guid GroupRequestId { get; set; }

        // Статус доступности(открытая/закрытая) 
        public bool StatusAccess { get; set; } = false;
        // Количество участников
        public int CountMembers { get; set; } 
        // Ссылка - приглашение
        public string LinkInvitation { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }
}
