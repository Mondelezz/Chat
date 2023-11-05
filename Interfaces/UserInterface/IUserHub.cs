using Quantum.Models.DTO;

namespace Quantum.Interfaces.UserInterface
{
    public interface IUserHub
    {
        public Task EnterUserInformation(UserDTO userDTO);
    }
}
