using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Quantum.Data;
using Quantum.Interfaces.UserInterface;
using Quantum.Models;

namespace Quantum.Services.UserServices
{
    public class ContactFriendlyService : IFriendAction
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ContactFriendlyService> _logger;
        private readonly IMapper _mapper;
        private readonly JwtTokenProcess _jwtTokenProcess;
        public ContactFriendlyService(ILogger<ContactFriendlyService> logger, DataContext dataContext, IMapper mapper, JwtTokenProcess jwtTokenProcess)
        {
            _logger = logger;
            _dataContext = dataContext;
            _mapper = mapper;
            _jwtTokenProcess = jwtTokenProcess;
        }

        /// <summary>
        /// Поиск пользователя среди зарегистрированных  пользователей
        /// </summary>
        /// <param name="phoneNumber">
        /// Номер телефона
        /// </param>
        public async Task<UserInfoOutput> SearchUser(string phoneNumber)
        {
            
            User? userReceiver = await _dataContext.Users.FirstOrDefaultAsync(pN => pN.PhoneNumber == phoneNumber);
            if (userReceiver != null)
            {
                _logger.Log(LogLevel.Information, $"Пользователь получатель:{userReceiver.UserName}");
                UserInfoOutput userReceiverMapper = _mapper.Map<UserInfoOutput>(userReceiver);
                return userReceiverMapper;
            }
            throw new Exception("Пользователь не найден");      
        }


        /// <summary>
        /// Добавление пользователя в друзья
        /// </summary>
        /// <param name="phoneNumber">
        /// Номер телефона
        /// </param>
        /// <param name="authHeaderValue">
        /// Заголовок, содержащий jwtToken
        /// </param>
        public async Task AddFriendInContact(string phoneNumber, string authHeaderValue)
        {
            UserInfoOutput userReceiver = await SearchUser(phoneNumber);

            bool result = await FriendExists(authHeaderValue, userReceiver.UserId);
            
            if (userReceiver == null || result)
            {
                throw new Exception("Не удалось добавить пользователя в друзья");
            }

            Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            User userSender = await _dataContext.Users.FirstAsync(id => id.UserId == userId);
            _logger.Log(LogLevel.Information, $"Пользователь отправитель {userSender.UserName}");

            if (userReceiver.PhoneNumber == userSender.PhoneNumber)
            {
                throw new Exception("Не удалось добавить пользователя в друзья");
            }
            UserFriends userFriends = new UserFriends()
            {
                UserId = userId,
                FriendId = userReceiver.UserId
            };
                  
            DbSet<UserFriends> userFriendsSet = _dataContext.Set<UserFriends>();
            await userFriendsSet.AddAsync(userFriends); 
            
            await _dataContext.SaveChangesAsync();

            _logger.Log(LogLevel.Information, $"Пользователь {userReceiver.UserName} добавлен в друзья к пользователю {userSender.UserName}");
        }

        /// <summary>
        /// Проверка на существование пользователя в друзьях
        /// </summary>
        /// <param name="authHeaderValue">
        /// Заголовок, содержащий jwtToken
        /// </param>
        /// <param name="friendId">
        /// Id друга
        /// </param>
        private async Task<bool> FriendExists(string authHeaderValue, Guid friendId)
        {
            Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            DbSet<UserFriends> userFriends = _dataContext.Set<UserFriends>();
            return await userFriends.AnyAsync(uf => uf.UserId == userId && uf.FriendId == friendId);
        }
    }
}
