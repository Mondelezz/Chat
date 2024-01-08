using Quantum.UserP.Models;
using System.Text.Json.Serialization;

namespace Quantum.GroupFolder.Models
{
    public class Group
    {
        // Название
        public string NameGroup { get; set; } = string.Empty;
        // Описание
        public string? DescriptionGroup { get; set; } = string.Empty;
        // Id
        public Guid GroupId { get; set; }
        // Участники
        [JsonIgnore]
        public ICollection<UserGroups> Members { get; set; } = new List<UserGroups>();
        // Заявки
        public GroupRequest Requests { get; set; } = new GroupRequest();
        // Статус доступности(открытая/закрытая) 
        public bool StatusAccess { get; set; } = false;
        // Количество участников
        public int CountMembers { get; set; } 
        // Ссылка - приглашение
        public string LinkInvitation { get; set; } = string.Empty;
    }
}
