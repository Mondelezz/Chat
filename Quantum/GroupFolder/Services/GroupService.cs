using Quantum.Data;
using Quantum.GroupFolder.Enums;
using Quantum.Group.GroupInterface;
using System;

namespace Quantum.GroupFolder.Services.Group
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
        public Task CreateGroup(string nameGroup, string descriptionGroup, AccessGroup access)
        {
            Guid groupId = Guid.NewGuid();
            string linkInvitation = LinkGeneration(groupId);

            switch (access)
            {
                case AccessGroup.Open:
                    {
                        Group group = new Group 
                    }
                    break;
                case AccessGroup.Closed:
                    {
                        
                    }
                    break;
                default:
                    break;
            }
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
