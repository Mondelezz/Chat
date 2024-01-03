using Quantum.Data;
using Quantum.Group.GroupInterface;
using System;

namespace Quantum.Services.Group
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
        // Доступ
        public enum Access 
        {
            Open = 1,
            Closed = 2 
        }

        public Task CreateGroup(string nameGroup, string descriptionGroup, Access access)
        {
            Guid groupId = Guid.NewGuid();
            string linkInvitation = LinkGeneration(groupId);

            switch (access)
            {
                case Access.Open:
                    {
                        
                    }
                    break;
                case Access.Closed:
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
