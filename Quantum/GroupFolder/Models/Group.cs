using Quantum.UserP.Models;
using StackExchange.Redis;

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
        public ICollection<UserGroups> Members { get; set; } = new List<UserGroups>();
        // Статус доступности(открытая/закрытая) 
        public bool StatusAccess { get; set; } = false;
        // Количество участников
        public int CountMembers { get; set; } 
        // Ссылка - приглашение
        public string LinkInvitation { get; set; } = string.Empty;
    }
}
