using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.GroupInterface;
using Quantum.GroupFolder.Models;
using Quantum.Services;

namespace Quantum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly ICreateGroup _createGroup;
        private readonly JwtTokenProcess _jwtTokenProcess;
        private readonly ILogger<GroupController> _logger;
        public GroupController(ICreateGroup createGroup, JwtTokenProcess jwtTokenProcess, ILogger<GroupController> logger)
        { 
            _createGroup = createGroup;
            _jwtTokenProcess = jwtTokenProcess;
            _logger = logger;
        }
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        public async Task<ActionResult<Group>> CreateGroup([FromBody]string nameGroup, string? descriptionGroup, AccessGroup access)
        {
            string token = ExtractAuthTokenFromHeaders();
            _logger.Log(LogLevel.Information, token);

            Guid creatorId = _jwtTokenProcess.GetUserIdFromJwtToken(token);
            _logger.Log(LogLevel.Information, creatorId.ToString());

            Group group = await _createGroup.CreateGroupAsync(nameGroup, descriptionGroup, creatorId, access);
            if (group == null)
            {
                return BadRequest("Не удалось создать группу.");
            }
            _logger.Log(LogLevel.Information, $"Группа {nameGroup} создана");
            return Ok(group);          
        }
    }
}
