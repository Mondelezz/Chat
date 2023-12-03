using Quantum.Models;

namespace Quantum.Interfaces.UserInterface
{
    public interface IFriendAction
    {
        public Task AddFriendInContact(string phoneNumber, string authHeaderValue);
        public Task<UserInfoOutput> SearchUser(string phoneNumber);
    }
}
