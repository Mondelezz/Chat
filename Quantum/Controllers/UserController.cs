using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Quantum.Interfaces.UserInterface;
using Quantum.Models;
using Quantum.Models.DTO;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserHub _userHub;
        private readonly IFriendAction _friendAction;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserHub userHub, ILogger<UserController> logger, IFriendAction friendAction)
        {
            _userHub = userHub;
            _logger = logger;
            _friendAction = friendAction;
        }

        // Получение токена из заголовка авторизации
        private string ExtractAuthTokenFromHeaders()
        {
            try
            {
                string token = HttpContext.Request.Headers["Authorization"]!;
                return token;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Заголовок отсутствует{ex.Message}");
                throw;
            }

        }
        [HttpPost("reg")]
        public async Task EnterUserInformation([FromBody] RegistrationUserDTO registrationUserDTO)
        {
            try
            {
                await _userHub.EnterUserInformationAsync(registrationUserDTO);                
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;                
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("upInfo")]
        public async Task<ActionResult> UpdateUserInformation([FromBody] UsersOpenData updateInfo)
        {
            try
            {
                string token = ExtractAuthTokenFromHeaders();
                UserInfoOutput result = await _userHub.UserUpdateInfoAsync(updateInfo, token);
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("addFriends")]
        public async Task<IActionResult> AddFriends(string phoneNumber)
        {
            string jwtToken = ExtractAuthTokenFromHeaders();
            await _friendAction.AddFriendInContact(phoneNumber, jwtToken);
            return Ok("Пользователь добавлен в друзья");

        }
    }
}
