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
        public async Task AddFriendInContact(string phoneNumber, string authHeaderValue)
        {
            UserInfoOutput userReceiver = await SearchUser(phoneNumber);
            if (userReceiver == null)
            {     
                return;
            }
            Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            User userSender = await _dataContext.Users.FirstAsync(id => id.UserId == userId);
            _logger.Log(LogLevel.Information, $"Пользователь отправитель {userSender.UserName}");

            Friends friends = new Friends();
            friends.UserId = userId;
            friends.FriendId = userReceiver.UserId;

            await _dataContext.Friends.AddAsync(friends);
            _logger.Log(LogLevel.Information, $"Пользователь {userReceiver.UserName} добавлен в друзья к пользователю {userSender.UserName}");

            await _dataContext.SaveChangesAsync();
        }     
    }
}
