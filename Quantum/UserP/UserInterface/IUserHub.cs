using Quantum.Models.DTO;
using Quantum.UserP.Models;

namespace Quantum.UserP.UserInterface
{
    public interface IUserHub
    {
        public Task EnterUserInformationAsync(RegistrationUserDTO registrationUserDTO);
        public Task<UserInfoOutput> UserUpdateInfoAsync(UserOpenData updateInfo, string token);
    }
}
