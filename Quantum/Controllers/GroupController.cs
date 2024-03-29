﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quantum.Data;
using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.GroupInterface;
using Quantum.GroupFolder.Models;
using Quantum.Services;
using Quantum.UserP.Models;

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
        [HttpPost("request")]
        public async Task<ActionResult<bool>> Join(Guid groupId)
        {
            string authHeaderValue = ExtractAuthTokenFromHeaders();
            _logger.Log(LogLevel.Information, authHeaderValue);

            Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            _logger.Log(LogLevel.Information, userId.ToString());

            Group group = await _dataContext.Groups.FirstAsync(i => i.GroupId == groupId);
            if (group == null) 
            { 
                return NotFound();
            }
            if (group.StatusAccess)
            {
                bool result = await _handleMembers.SendRequestOpenGroupAsync(groupId, userId);
                if (result)
                {
                    return Ok("Вы успешно вступили в группу");
                }
                return BadRequest("не удалось вступить в группу");
            }
            else
            {
                bool result = await _handleMembers.SendRequestClosedGroupAsync(groupId, userId);
                if (result)
                {
                    return Ok("Заявка отправлена");
                }
                return BadRequest("Не удалось отправить заявку.");
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("invite")]
        public async Task<ActionResult<bool>> Invite(Guid groupId, Guid receiverId)
        {
            string authHeaderValue = ExtractAuthTokenFromHeaders();
            _logger.Log(LogLevel.Information, authHeaderValue);

            Guid senderId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            _logger.Log(LogLevel.Information, "Айди отправителя приглашнеия: " + senderId.ToString());

            bool result = await _handleMembers.AddMembersAsync(
                groupId: groupId,
                senderId: senderId,
                receiverId: receiverId);
            if (result)
            {
                return Ok("Приглашение отправлено.");
            }
            return BadRequest("Не удалось отправить приглшашение.");
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("handle_request")]
        public async Task<ActionResult<bool>> AcceptRequest(Guid groupId, Guid userId)
        {
            string authHeaderValue = ExtractAuthTokenFromHeaders();
            _logger.Log(LogLevel.Information, authHeaderValue);

            Guid ownerId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            _logger.Log(LogLevel.Information, "Айди владельца: " + ownerId.ToString());

            await _handleMembers.AcceptRequestsAsync(ownerId, groupId, userId);
            bool result = await _handleMembers.SendRequestOpenGroupAsync(groupId, userId);
            if (result)
            {
                return Ok("Согласие");
            }
            return BadRequest("Отказ");
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("del_user")]
        public async Task<ActionResult<bool>> RemoveUserFromGroupAsync(Guid groupId, Guid delUserId)
        {
            string authHeaderValue = ExtractAuthTokenFromHeaders();
            _logger.Log(LogLevel.Information, authHeaderValue);

            Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            _logger.Log(LogLevel.Information, "Айди пользователя: " + userId.ToString());
            bool result = await _handleMembers.RemoveUserFromGroupAsync(groupId, userId, delUserId);
            if (result)
            {
                return Ok("Пользователь удалён.");
            }
            return BadRequest("Не удалось удалить пользователя.");
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("upRole")]
        public async Task<ActionResult<GroupUserRole>> UpRoleAsync(Guid groupId, Guid userId)
        {
            string authHeaderValue = ExtractAuthTokenFromHeaders();
            _logger.Log(LogLevel.Information, authHeaderValue);

            Guid ownerId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            _logger.Log(LogLevel.Information, "Айди владельца: " + ownerId.ToString());

            GroupUserRole groupUserRole = await _handleMembers.UpRoleAsync(groupId, ownerId, userId);
            if (groupUserRole != null) 
            {
                return Ok(groupUserRole);
            }
            return BadRequest("Не удалось повысить в роли.");
        }
    }
}
