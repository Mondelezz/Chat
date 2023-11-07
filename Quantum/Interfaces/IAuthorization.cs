using Quantum.Models.DTO;

namespace Quantum.Interfaces
{
    public interface IAuthorization
    {
        public Task<string> AuthenticateUser(AuthorizationUserDTO authorizationUserDTO);
    }
}
