using Quantum.UserP.Models;

namespace Quantum.UserP.UserInterface
{
    public interface IFriendAction
    {
        public Task AddFriendInContact(string phoneNumber, string authHeaderValue);
        public Task<UserInfoOutput> SearchUser(string phoneNumber);
    }
}
