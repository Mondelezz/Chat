using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quantum.Interfaces;
using Quantum.Models.DTO;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthorization _authorization;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthorization authorization, ILogger<AuthController> logger)
        {
            _authorization = authorization;
            _logger = logger;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<string>> AuthorizationUser([FromBody] AuthorizationUserDTO authorizationUserDTO)
        {
            if (authorizationUserDTO != null)
            {
                try
                {
                    string jwtToken = await _authorization.AuthenticateUserAsync(authorizationUserDTO);
                    return Ok(jwtToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка аутентификации пользователя: {ex.Message}");
                    return BadRequest("Неверный логин или пароль.");
                }               
            }
            return Unauthorized();
        }
    }
}
