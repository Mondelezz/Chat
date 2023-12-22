using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quantum.Interfaces.UserInterface;
using Quantum.Models;
using Quantum.Models.DTO;
using Quantum.Services;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserHub _userHub;
        private readonly IFriendAction _friendAction;
        private readonly JwtTokenProcess _jwtTokenProcess;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserHub userHub, ILogger<UserController> logger, IFriendAction friendAction, JwtTokenProcess jwtTokenProcess)
        {
            _userHub = userHub;
            _logger = logger;
            _friendAction = friendAction;
            _jwtTokenProcess = jwtTokenProcess;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("info")]
        
        public ActionResult<List<UsersOpenData>> GetInfoUser(string jwtToken)
        {
            List<UsersOpenData> userData = _jwtTokenProcess.GetUserInfo("Bearer " + jwtToken);
            if (userData.IsNullOrEmpty())
            {
                return NotFound();
            }
            return Ok(userData);
        }
    }
}
