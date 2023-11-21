using Quantum.Models.DTO;

namespace Quantum.Interfaces
{
    public interface IAuthorization
    {
        public Task<string> AuthenticateUserAsync(AuthorizationUserDTO authorizationUserDTO);
    }
}
