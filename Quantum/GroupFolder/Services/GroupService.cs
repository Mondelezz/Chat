using AutoMapper;
using AutoMapper.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Quantum.Data;
using Quantum.GroupFolder.Enums;
using Quantum.GroupFolder.GroupInterface;
using Quantum.GroupFolder.Models;
using Quantum.UserP.Models;
using System.Text.RegularExpressions;
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
                _logger.Log(LogLevel.Warning, "Человек, отправляющий приглашение не находится в группе");
                return false;
            }
            // Добавление друга в ОТКРЫТУЮ группу
            if (group.StatusAccess)
            {
                GroupUserRole newUserGroups = new GroupUserRole
                {
                    GroupId = groupId,
                    UserId = receiverId,
                    Role = RolesGroupType.Member
                };
                _logger.Log(LogLevel.Information, $"GroupId: {groupId}\n UserId: {receiverId}\n Role: {RolesGroupType.Member}");
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
            // Добавление друга в ЗАКРЫТУЮ группу
            else if(!group.StatusAccess)
            {
                // Доступ - закрытая: При приглашении друга - добавлять пользователя в заявки, о разрешении на принятие в группу.
                User user = await _dataContext.Users.FirstAsync(id => id.UserId == receiverId);
                if (user == null)
                {
                    _logger.Log(LogLevel.Warning, "Пользователя, которого приглашают не существует");
                    return false;
                }
                UserInfoOutput userInfoOutput = _mapper.Map<UserInfoOutput>(user);
                group.Requests.Users?.Add(userInfoOutput);
                group.Requests.CountRequsts++;


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
   
        public async Task<bool> SendRequestClosedGroup(Guid groupId, Guid senderId)
        {
            Group group = await _dataContext.Groups.FirstAsync(id => id.GroupId == groupId);
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
            UserInfoOutput userInfoOutput = _mapper.Map<UserInfoOutput>(user);
            group.Requests.Users?.Add(userInfoOutput);
            group.Requests.CountRequsts++;
            return true;
            // Заявка отправлена
        }


        public async Task<bool> SendRequestOpenGroup(Guid groupId, Guid senderId)
        {
            Group group = await _dataContext.Groups.FirstAsync(id => id.GroupId == groupId);
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

            GroupUserRole newUserGroups = new GroupUserRole
            {
                GroupId = groupId,
                UserId = senderId,
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
    }
}
