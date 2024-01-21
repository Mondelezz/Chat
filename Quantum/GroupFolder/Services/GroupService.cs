using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Buffers;
using Quantum.Data;
using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.GroupInterface;
using Quantum.GroupFolder.Models;
using Quantum.UserP.Models;
using Group = Quantum.GroupFolder.Models.Group;

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
                CountMembers = 1,
                GroupRequestId = Guid.NewGuid(),
                CreatedTime = DateTime.UtcNow,
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

            bool result = await SaveToDatabaseAsync(group, newGroupUserRole);
            if (!result)
            {
                throw new ArgumentException("Ошибка сохранения в базу данных");
            }
            _logger.Log(LogLevel.Information, "Данные сохранены");
            return group;
        }
        /// <summary>
        /// Добалвение владельца в группу, как первого участника + назначение роли
        /// </summary>
        /// <param name="groupId">
        /// Айди группы
        /// </param>
        /// <param name="creatorId">
        /// Айди создателя
        /// </param>
        /// <returns>Добавление владельца</returns>
        public GroupUserRole AddCreator(Guid groupId, Guid creatorId)
        {
            GroupUserRole newUserGroups = new GroupUserRole
            {
                GroupId = groupId,
                UserId = creatorId,
                JoinDate = DateTime.UtcNow,
                Role = RolesGroupType.Owner
            };
            _logger.Log(LogLevel.Information, $"GroupId: {groupId}\n UserId: {creatorId}\n Role: {RolesGroupType.Owner}");
            return newUserGroups;
        }
        private async Task<bool> SaveToDatabaseAsync(Group group, GroupUserRole newGroupUserRole)
        {
            _logger.Log(LogLevel.Information, "Добавление группы в бд");
            await _dataContext.Groups.AddAsync(group);

            _logger.Log(LogLevel.Information, "Добавление создателя и его роли в бд");
            await _dataContext.GroupUserRole.AddAsync(newGroupUserRole);

            DbSet<UserGroups> userGroupsDb = _dataContext.Set<UserGroups>();
            UserGroups userGroups = _mapper.Map<UserGroups>(newGroupUserRole);

            _logger.Log(LogLevel.Information, "Добавление создателя в группу");
            group.Members.Add(userGroups);

            _logger.Log(LogLevel.Information, "Добавление группы к пользователю в бд");
            await userGroupsDb.AddAsync(userGroups);

            _logger.Log(LogLevel.Information, "Добавление заявок группы в бд");
            GroupRequest groupRequest = new GroupRequest();
            groupRequest.GroupId = group.GroupId;

            _logger.Log(LogLevel.Information, "Сохранение данных");
            await _dataContext.SaveChangesAsync();

            return true;

        }
        /// <summary>
        /// Приглашение участников в открытую и закрытую группу (только друзья)
        /// </summary>
        /// <param name="groupId"></param>
        /// Айди группы
        /// <param name="senderId">
        /// Айди отправителя (кто приглашает)
        /// </param>  
        /// <param name="receiverId">
        /// Айди получателя (приглашённый)
        /// </param>
        /// <returns> Отправлена/ Не отправлена заявка </returns> 
        public async Task<bool> AddMembersAsync(Guid groupId, Guid senderId, Guid receiverId)
        {

            DbSet<UserFriends> userFriendsDb = _dataContext.Set<UserFriends>();

            UserFriends userFriends = await userFriendsDb.AsNoTracking().FirstAsync(u => u.UserId == senderId && u.FriendId == receiverId);
            if (userFriends == null)
            {
                _logger.Log(LogLevel.Warning, "Пользователи не найдены в бд");
                return false;
            }

            Group group = await _dataContext.Groups.Include(u => u.Members).Include(gr => gr.GroupRequest).FirstAsync(id => id.GroupId == groupId);
            if (group == null)
            {
                _logger.Log(LogLevel.Warning, "Группы не существвует");
                return false;
            }
            bool groupFlags = group.Members.Any(u => u.UserId == senderId && u.GroupId == groupId);
            if (!groupFlags)
            {
                _logger.Log(LogLevel.Warning, "Человек, отправляющий приглашение не находится в группе");
                return false;
            }
            DbSet<UserGroups> userGroupsDb = _dataContext.Set<UserGroups>();
            UserGroups? userInGroup = await userGroupsDb.FirstOrDefaultAsync(id => id.GroupId == groupId && id.UserId == receiverId);
            if (userInGroup != null)
            {
                throw new Exception("Пользователь уже в группе.");
            }
            // Добавление друга в ОТКРЫТУЮ группу
            if (group.StatusAccess)
            {
                GroupUserRole newUserGroups = new GroupUserRole
                {
                    GroupId = groupId,
                    UserId = receiverId,
                    JoinDate = DateTime.UtcNow,
                    Role = RolesGroupType.Member
                };
                _logger.Log(LogLevel.Information, $"GroupId: {groupId}\n UserId: {receiverId}\n Role: {RolesGroupType.Member}");
                await _dataContext.GroupUserRole.AddAsync(newUserGroups);

                UserGroups userGroups = _mapper.Map<UserGroups>(newUserGroups);

                group.Members.Add(userGroups);
                group.CountMembers++;
                
                await userGroupsDb.AddAsync(userGroups);
                _logger.Log(LogLevel.Information, "Пользователь добавлен");

                _dataContext.Groups.Update(group);
                _logger.Log(LogLevel.Information, "Данные обновлены");

                await _dataContext.SaveChangesAsync();
                return true;
            }
            // Добавление друга в ЗАКРЫТУЮ группу
            else if (!group.StatusAccess)
            {
                // Доступ - закрытая: При приглашении друга - добавлять пользователя в заявки, о разрешении на принятие в группу.
                User user = await _dataContext.Users.AsNoTracking().FirstAsync(id => id.UserId == receiverId);
                if (user == null)
                {
                    _logger.Log(LogLevel.Warning, "Пользователя, которого приглашают не существует");
                    return false;
                }

                UserInfoOutput userInfoOutput = _mapper.Map<UserInfoOutput>(user);

                GroupRequestUserInfoOutput groupRequestUserInfoOutput = new GroupRequestUserInfoOutput()
                {
                    UserInfoOutputId = receiverId,
                    GroupRequestId = group.GroupRequestId,
                    CreatedTime = DateTime.UtcNow,
                };

                DbSet<GroupRequestUserInfoOutput> groupRequestUserInfoOutputsDb = _dataContext.Set<GroupRequestUserInfoOutput>();

                await groupRequestUserInfoOutputsDb.AddAsync(groupRequestUserInfoOutput);

                group.GroupRequest.CountRequests++;
                group.GroupRequest.GroupId = groupId;

                await _dataContext.SaveChangesAsync();
                return true;
                // Заявка отправлена
            }
            else
            {
                throw new Exception("Неизвестная команда");
            }
        }
        /// <summary>
        /// Отправка заявки в закрытую группу
        /// </summary>
        /// <param name="groupId">
        /// Айди группы
        /// </param>
        /// <param name="senderId">
        /// Айди отправителя заявки
        /// </param>
        /// <returns> Отправлена/ Не отправлена заявка </returns> 

        // ДОДЕЛАТЬ
        public async Task<bool> SendRequestClosedGroupAsync(Guid groupId, Guid senderId)
        {
            Group group = await _dataContext.Groups.Include(ug => ug.Members).Include(gr => gr.GroupRequest).FirstAsync(id => id.GroupId == groupId);
            if (group == null)
            {
                _logger.Log(LogLevel.Warning, "Группы не существвует");
                return false;
            }

            User user = await _dataContext.Users.FirstAsync(id => id.UserId == senderId);
            if (user == null)
            {
                _logger.Log(LogLevel.Warning, "Пользователя не существует");
                return false;
            }
            DbSet<UserGroups> userGroupsDb = _dataContext.Set<UserGroups>();
            UserGroups? userInGroup = await userGroupsDb.FirstOrDefaultAsync(id => id.GroupId == groupId && id.UserId == senderId);
            if (userInGroup != null)
            {
                throw new Exception("Вы уже в группе.");
            }
            UserInfoOutput userInfoOutput = _mapper.Map<UserInfoOutput>(user);

            GroupRequestUserInfoOutput groupRequestUserInfoOutput = new GroupRequestUserInfoOutput()
            {
                UserInfoOutputId = senderId,
                GroupRequestId = group.GroupRequestId,
                CreatedTime = DateTime.UtcNow,
            };

            DbSet<GroupRequestUserInfoOutput> groupRequestUserInfoOutputsDb = _dataContext.Set<GroupRequestUserInfoOutput>();

            await groupRequestUserInfoOutputsDb.AddAsync(groupRequestUserInfoOutput);

            group.GroupRequest.CountRequests++;
            group.GroupRequest.GroupId = groupId;

            await _dataContext.SaveChangesAsync();
            return true;
            // Заявка отправлена
        }

        /// <summary>
        /// Отправка заявки в открытую группу
        /// </summary>
        /// <param name="groupId">
        /// Айди группы
        /// </param>
        /// <param name="senderId">
        /// Айди отправителя заявки
        /// </param>
        /// <returns> Вступил/ Не встуипл </returns> 
        public async Task<bool> SendRequestOpenGroupAsync(Guid groupId, Guid senderId)
        {
            Group group = await _dataContext.Groups.Include(ug => ug.Members).FirstAsync(id => id.GroupId == groupId);
            if (group == null)
            {
                _logger.Log(LogLevel.Warning, "Группы не существвует");
                return false;
            }

            User user = await _dataContext.Users.AsNoTracking().FirstAsync(id => id.UserId == senderId);
            if (user == null)
            {
                _logger.Log(LogLevel.Warning, "Пользователя не существует");
                return false;
            }

            GroupUserRole newUserGroups = new GroupUserRole
            {
                GroupId = groupId,
                UserId = senderId,
                JoinDate = DateTime.UtcNow,
                Role = RolesGroupType.Member
            };
            _logger.Log(LogLevel.Information, $"GroupId: {groupId}\n UserId: {senderId}\n Role: {RolesGroupType.Member}");
            await _dataContext.GroupUserRole.AddAsync(newUserGroups);

            UserGroups userGroups = _mapper.Map<UserGroups>(newUserGroups);

            group.Members.Add(userGroups);
            group.CountMembers++;

            DbSet<UserGroups> userGroupsDb = _dataContext.Set<UserGroups>();
            await userGroupsDb.AddAsync(userGroups);
            _logger.Log(LogLevel.Information, "Пользователь добавлен");

            _dataContext.Groups.Update(group);
            _logger.Log(LogLevel.Information, "Данные обновлены");

            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AcceptRequestsAsync(Guid ownerId, Guid groupId, Guid userId)
        {
            Group group = await _dataContext.Groups.AsNoTracking().Include(u => u.Members).FirstAsync(i => i.GroupId == groupId);
            if (group == null)
            {
                throw new Exception("Группа не найдена");
            }

            GroupUserRole groupUserRole = await _dataContext.GroupUserRole.FirstAsync
                (u => u.GroupId == group.GroupId
                    &&
                u.UserId == ownerId
                    &&
                u.Role == RolesGroupType.Owner);
            if (groupUserRole == null)
            {
                throw new Exception("Группа, владелец и роль не совпали.");
            }

            DbSet<GroupRequestUserInfoOutput> groupRequestUserInfoOutputsDb = _dataContext.Set<GroupRequestUserInfoOutput>();
            GroupRequestUserInfoOutput? request = await groupRequestUserInfoOutputsDb.FirstOrDefaultAsync(
                u => u.UserInfoOutputId == userId
                    &&
                u.GroupRequestId == group.GroupRequestId);
            if (request == null)
            {
                throw new Exception("Заявка не найдена");
            }
            groupRequestUserInfoOutputsDb.Remove(request);
            GroupRequest groupRequest = await _dataContext.GroupRequests.FirstAsync(gr => gr.GroupId == groupId);
            groupRequest.CountRequests--;

            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserFromGroupAsync(Guid groupId,Guid userId, Guid delUserId)
        {
            Group group = await _dataContext.Groups.Include(u => u.Members).FirstAsync(i => i.GroupId == groupId);
            if (group == null)
            {
                throw new Exception("Группа не найдена.");
            }
            GroupUserRole groupUserRole = await _dataContext.GroupUserRole.FirstAsync
               (u => u.GroupId == group.GroupId
                   &&
               u.UserId == userId 
                   &&
               u.Role == RolesGroupType.Owner || u.Role == RolesGroupType.Admin);
            if (groupUserRole == null)
            {
                throw new Exception("Недостаточно прав.");
            }

            DbSet<UserGroups> userGroupsDb = _dataContext.Set<UserGroups>();
            UserGroups? userGroups = await userGroupsDb.FirstOrDefaultAsync(id => id.GroupId == groupId && id.UserId == delUserId);
            if (userGroups == null)
            {
                throw new Exception("Пользователь не в группе.");
            }
            userGroupsDb.Remove(userGroups);
            group.CountMembers--;
            await _dataContext.SaveChangesAsync();
            return true;
        }

        public async Task<GroupUserRole> UpRoleAsync(Guid groupId, Guid ownerId, Guid userId)
        {
            Group group = await _dataContext.Groups.AsNoTracking().Include(u => u.Members).FirstAsync(g => g.GroupId == groupId);
            if (group == null)
            {
                throw new Exception("Группа не найдена.");
            }
            UserGroups? userGroups = group.Members.FirstOrDefault(u => u.UserId == userId);
            if (userGroups == null)
            {
                throw new Exception("Пользователь в группе не найден.");
            }
            GroupUserRole? roleError = userGroups.User?.Roles.FirstOrDefault(g => g.GroupId == groupId && (g.Role == RolesGroupType.Admin || g.Role == RolesGroupType.Owner) && g.UserId == userId);
            if (roleError != null)
            {
                throw new Exception("Невозможно повысить роль выше админа.");
            }
            GroupUserRole groupUserRole = await _dataContext.GroupUserRole.FirstAsync(o => o.UserId == ownerId && o.Role == RolesGroupType.Owner);
            if (groupUserRole == null)
            {
                throw new Exception("Не удовлетворяет условию для повышенимя роли.");
            }
            GroupUserRole groupUserRoleUpdate = await _dataContext.GroupUserRole.FirstAsync(u => u.UserId == userId);
            groupUserRoleUpdate.Role = RolesGroupType.Admin;
            _dataContext.GroupUserRole.Update(groupUserRoleUpdate);
            await _dataContext.SaveChangesAsync();
            return groupUserRoleUpdate;
        }
    }
}
