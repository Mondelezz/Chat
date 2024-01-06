using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Quantum.Data;
using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.GroupInterface;
using Quantum.GroupFolder.Models;
using Quantum.UserP.Models;
using System;

namespace Quantum.GroupFolder.Services
{
    public class GroupService : ICreateGroup, ILinkInvitation, IHandleMembers
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GroupService> _logger;

        public GroupService(DataContext dataContext, ILogger<GroupService> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }
        public string LinkGeneration(Guid groupId)
        {
            string linkInvitation = "https://qm.me/+" + groupId.ToString().Substring(0, 15).Replace('-', 'Q');
            return linkInvitation;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize]
        public async Task<Group> CreateGroupAsync(string nameGroup, string? descriptionGroup, Guid creatorId, AccessGroup access)
        {
            Guid groupId = Guid.NewGuid();
            string linkInvitation = LinkGeneration(groupId);
            Group group = new Group()
            {
                GroupId = groupId,
                LinkInvitation = linkInvitation,
                NameGroup = nameGroup,
                DescriptionGroup = descriptionGroup,
                CountMembers = 1
              
            };
            switch (access)
            {
                case AccessGroup.Open:
                    {
                        group.StatusAccess = true;
                    }
                    break;
                case AccessGroup.Closed:
                    {
                        group.StatusAccess = false;
                    }
                    break;
                default:
                    throw new ArgumentException("Неизвестная ошибка.");
            }
            UserGroups newUserGroups = AddCreator(groupId, creatorId);
            _logger.Log(LogLevel.Information, $"creatorId: {newUserGroups.UserId} groupId: {newUserGroups.GroupId}");

            bool result = await SaveInDataBase(group, newUserGroups);
            if (!result)
            {
                throw new ArgumentException("Ошибка сохранения в базу данных");
            }
            _logger.Log(LogLevel.Information, "Данные сохранены");
            return group;
        }
        private async Task<bool> SaveInDataBase(Group group, UserGroups newUserGroups)
        {
            await _dataContext.AddAsync(group);

            DbSet<UserGroups> userGroups = _dataContext.Set<UserGroups>();
            await userGroups.AddAsync(newUserGroups);

            await _dataContext.SaveChangesAsync();
            bool saved = true;

            if (!saved)
            {
                return false;
            }
            return true;

        }
        public UserGroups AddCreator(Guid groupId, Guid creatorId)
        {
            UserGroups newUserGroups = new UserGroups
            {
                GroupId = groupId,
                UserId = creatorId
            };
            return newUserGroups;          
        }
        
        public Task AddMembersAsync(Guid groupId, Guid senderId, Guid receiverId)
        {
            throw new NotImplementedException();
        }             
    }
}
