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
        public async Task<UserInfoOutput> SearchUser(string phoneNumber, string authHeaderValue)
        {
            Guid userId =  _jwtTokenProcess.GetUserIdFromJwtToken(authHeaderValue);
            User? userReceiver = await _dataContext.Users.FirstOrDefaultAsync(pN => pN.PhoneNumber == phoneNumber);
            UserInfoOutput userData = _mapper.Map<UserInfoOutput>(userReceiver);
            return userData;
        }
        public async Task AddFriendInContact(string phoneNumber)
        {
            UserInfoOutput result = await SearchUser(phoneNumber);
            if (result == null)
            {     
                return;
            }

        }

        
    }
}
