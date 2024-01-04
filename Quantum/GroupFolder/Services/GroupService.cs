using Quantum.Data;
using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.GroupInterface;
using Quantum.GroupFolder.Models;
using System;

namespace Quantum.GroupFolder.Services
{
    public class GroupService : ICreateGroup, ILinkInvitation
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GroupService> _logger;

        public GroupService(DataContext dataContext, ILogger<GroupService> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }             
        public async Task<Group> CreateGroup(string nameGroup, string descriptionGroup, AccessGroup access)
        {
            Guid groupId = Guid.NewGuid();
            string linkInvitation = LinkGeneration(groupId);
            Group group = new Group()
            {
                GroupId = groupId,
                LinkInvitation = linkInvitation,
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
            await SaveInDataBase(group);
            return group;
        }
        private async Task<bool> SaveInDataBase(Group group)
        { 
            await _dataContext.AddAsync(group);
            await _dataContext.SaveChangesAsync();
            bool saved = true;

            if (!saved)
            {
                return false;
            }
            return true;
            
        }
        public Task AddParticipants(Guid authorId, Guid friendId)
        {
            throw new NotImplementedException();
        }

        public string LinkGeneration(Guid groupId)
        {
            string linkInvitation = "https://qm.me/+" + groupId.ToString().Substring(0, 15).Replace('-', 'Q');
            return linkInvitation;
        }
    }
}
