using Quantum.GroupFolder.Models;

namespace Quantum.GroupFolder.GroupInterface
{
    public interface IHandleMembers
    {
        public Task<bool> AddMembersAsync(Guid groupId, Guid senderId, Guid receiverId);
        public Task<bool> SendRequestClosedGroup(Guid groupId, Guid senderId);
        public Task<bool> SendRequestOpenGroup(Guid groupId, Guid senderId);
        public GroupUserRole AddCreator(Guid groupId, Guid creatorId);
    }
}
