using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantum.Interfaces.UserInterface;
using Quantum.Models.DTO;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserHub _userHub;
        public UserController(IUserHub userHub)
        {
            _userHub = userHub;
        }
        [HttpPost("reg")]
        public async Task EnterUserInformation([FromBody] UserDTO userDTO)
        {
            try
            {
                await _userHub.EnterUserInformation(userDTO);                
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;                
            }
        }
    }
}
