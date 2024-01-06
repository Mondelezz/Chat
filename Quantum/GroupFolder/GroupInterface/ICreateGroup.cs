using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.Models;

namespace Quantum.GroupFolder.GroupInterface
{
    public interface ICreateGroup
    {
        public Task<Group> CreateGroupAsync(string nameGroup, string? descriptionGroup, Guid creatorId, AccessGroup access);       
    }
}
