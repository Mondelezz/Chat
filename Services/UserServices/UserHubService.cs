using AutoMapper;
using Quantum.Data;
using Quantum.Interfaces.UserInterface;
using Quantum.Models;
using Quantum.Models.DTO;

namespace Quantum.Services.UserServices
{
    public class UserHubService : IUserHub
    {
        private readonly ILogger<UserHubService> _logger;
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        public UserHubService(DataContext dataContext, IMapper mapper, ILogger<UserHubService> logger)
        { 
            _dataContext = dataContext;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task EnterUserInformation(UserDTO userDTO)
        {
            try
            {
                if (userDTO != null)
                {
                    User user = _mapper.Map<User>(userDTO);
                    user.UserId = Guid.NewGuid();

                    await AddUserToDatabase(user);

                    _logger.Log(LogLevel.Information, "Данные были сохранены в базу данных");
                }
                else
                {
                    _logger.Log(LogLevel.Warning, "Введённые данные отсутствуют");
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Исключение при сохранении в базу данных: {ex.Message}");
                throw;
            }
        }
        private async Task AddUserToDatabase(User user)
        {
            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();
        }
    }
}
