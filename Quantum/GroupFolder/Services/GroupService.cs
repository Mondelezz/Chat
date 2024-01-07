using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Quantum.Data;
using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.GroupInterface;
using Quantum.GroupFolder.Models;
using Quantum.UserP.Models;

namespace Quantum.GroupFolder.Services
{
    public class GroupService : ICreateGroup, ILinkInvitation, IHandleMembers
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GroupService> _logger;
        private readonly IMapper _mapper;
        public GroupService(DataContext dataContext, ILogger<GroupService> logger, IMapper mapper)
        {
            _dataContext = dataContext;
            _logger = logger;
            _mapper = mapper;
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
            GroupUserRole newGroupUserRole = AddCreator(groupId, creatorId);
            _logger.Log(LogLevel.Information, $"creatorId: {newGroupUserRole.UserId} groupId: {newGroupUserRole.GroupId}");

            bool result = await SaveToDatabase(group, newGroupUserRole);
            if (!result)
            {
                throw new ArgumentException("Ошибка сохранения в базу данных");
            }
            _logger.Log(LogLevel.Information, "Данные сохранены");
            return group;
        }

        public GroupUserRole AddCreator(Guid groupId, Guid creatorId)
        {
            GroupUserRole newUserGroups = new GroupUserRole
            {
                GroupId = groupId,
                UserId = creatorId,
                Role = RolesGroupType.Owner
            };
            _logger.Log(LogLevel.Information, $"GroupId: {groupId}\n UserId: {creatorId}\n Role: {RolesGroupType.Owner}");
            return newUserGroups;
        }
        private async Task<bool> SaveToDatabase(Group group, GroupUserRole newGroupUserRole)
        {
            if (group == null || newGroupUserRole == null )
            {
                return false;
            }
            _logger.Log(LogLevel.Information, "Добавление группы в бд");
            await _dataContext.Groups.AddAsync(group);

            _logger.Log(LogLevel.Information, "Добавление создателя и его роли в бд");
            await _dataContext.GroupUserRole.AddAsync(newGroupUserRole);

            DbSet<UserGroups> userGroupsDb = _dataContext.Set<UserGroups>();
            UserGroups userGroups = _mapper.Map<UserGroups>(newGroupUserRole);

            _logger.Log(LogLevel.Information, "Добавление группы к пользователю в бд");
            await userGroupsDb.AddAsync(userGroups);

            _logger.Log(LogLevel.Information, "Сохранение данных");
            await _dataContext.SaveChangesAsync();
            bool saved = true;

            if (!saved)
            {
                return false;
            }
            return true;

        }
              
        public async Task<bool> AddMembersAsync(Guid groupId, Guid senderId, Guid receiverId)
        {

            DbSet<UserFriends> userFriendsDb = _dataContext.Set<UserFriends>();

            UserFriends userFriends = await userFriendsDb.FirstAsync(u => u.UserId == senderId && u.FriendId == receiverId);
            if (userFriends == null)
            {
                _logger.Log(LogLevel.Warning, "Пользователи не найдены в бд");
                return false;
            }
            Group group = await _dataContext.Groups.FirstAsync(id => id.GroupId == groupId);
            if (group == null)
            {
                _logger.Log(LogLevel.Warning, "Группы не существвует");
                return false;
            }
            if (!group.Members.Any(u => u.UserId == senderId && u.GroupId == groupId))
            {
                _logger.Log(LogLevel.Warning, "Отправитель не находится в группе");
                return false;
            }
            if (group.StatusAccess)
            {
                UserGroups userGroups = new UserGroups()
                {
                    GroupId = groupId,
                    UserId = receiverId,
                };
                group.Members.Add(userGroups);
                group.CountMembers++;
                return true;
            }
            else if(!group.StatusAccess)
            {
                // Доступ - закрытая: При приглашении друга - отправлять уведомление владельцу, о разрешении на принятие в группу.
                if (group.Requests != null)
                {
                    foreach (UserInfoOutput user in group.Requests.Users)
                    {

                    }
                }
            }
            else
            {
                throw new Exception("Неизвестная команда");
            }
            
        }             
    }
}
