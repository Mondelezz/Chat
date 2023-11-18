using Quantum.Models;
using Quantum.Models.DTO;

namespace Quantum.Interfaces.UserInterface
{
    public interface IUserHub
    {
        public Task EnterUserInformation(RegistrationUserDTO registrationUserDTO);
        public Task<UserInfoOutput> UserUpdateInfoAsync(UpdateInfo updateInfo, string token);
    }
}
