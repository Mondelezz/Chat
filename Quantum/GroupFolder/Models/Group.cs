using Quantum.UserP.Models;
using StackExchange.Redis;

namespace Quantum.GroupFolder.Models
{
    public class Group
    {
        public int Id { get; set; }
        // Название
        public string NameGroup { get; set; } = string.Empty;
        // Описание
        public string? DescriptionGroup { get; set; } = string.Empty;
        // Id
        public Guid GroupId { get; set; }
        // Участники
        public List<UserInfoOutput> Members { get; set; } = new List<UserInfoOutput>();
        // Статус доступности(открытая/закрытая) 
        public bool StatusAccess { get; set; } = false;
        // Количество участников
        public int CountMembers { get; set; } 
        // Ссылка - приглашение
        public string LinkInvitation { get; set; } = string.Empty;
    }
}
