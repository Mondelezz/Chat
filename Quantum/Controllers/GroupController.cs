using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quantum.Data;
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
        private readonly IHandleMembers _handleMembers;
        private readonly JwtTokenProcess _jwtTokenProcess;
        private readonly ILogger<GroupController> _logger;
        private readonly DataContext _dataContext;
        public GroupController(
            ICreateGroup createGroup,
            IHandleMembers handleMembers,
            JwtTokenProcess jwtTokenProcess,
            ILogger<GroupController> logger,
            DataContext dataContext)
        { 
            _createGroup = createGroup;
            _handleMembers = handleMembers;
            _jwtTokenProcess = jwtTokenProcess;
            _logger = logger;
            _dataContext = dataContext;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("requst")]
        public async Task<ActionResult<bool>> Join(Guid groupId)
        {
            string token = ExtractAuthTokenFromHeaders();
            _logger.Log(LogLevel.Information, token);

            Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(token);
            _logger.Log(LogLevel.Information, userId.ToString());

            Group group = await _dataContext.Groups.FirstAsync(i => i.GroupId == groupId);
            if (group == null) 
            { 
                return NotFound();
            }
            if (group.StatusAccess)
            {
                bool result = await _handleMembers.SendRequestOpenGroup(groupId, userId);
                if (result)
                {
                    return Ok("Вы успешно вступили в группу");
                }
                return BadRequest();
            }
            else
            {
                bool result = await _handleMembers.SendRequestClosedGroup(groupId, userId);
                if (result)
                {
                    return Ok("Заявка отправлена");
                }
                return BadRequest();
            }
        }
    }
}
