using Quantum.GroupFolder.Models;

namespace Quantum.GroupFolder.GroupInterface
{
    public interface IHandleMembers
    {
        public Task AddMembersAsync(Guid groupId, Guid senderId, Guid receiverId);
        public GroupUserRole AddCreator(Guid groupId, Guid creatorId);
    }
}
