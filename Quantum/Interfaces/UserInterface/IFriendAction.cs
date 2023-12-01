using Quantum.Models;

namespace Quantum.Interfaces.UserInterface
{
    public interface IFriendAction
    {
        public Task AddFriendInContact(string phoneNumber);
        public Task<UserInfoOutput> SearchUser(string phoneNumber);
    }
}
