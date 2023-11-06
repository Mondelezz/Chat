using AutoMapper;
using Quantum.Data;
using Quantum.Interfaces.UserInterface;
using Quantum.Models;
using Quantum.Models.DTO;
using System.ComponentModel.DataAnnotations;

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
        public async Task EnterUserInformation(RegistrationUserDTO registrationUserDTO)
        {
            if (registrationUserDTO == null)
            {
                _logger.Log(LogLevel.Warning, "Введённые данные отсутствуют");
                return;
            }
            try
            {
                await AddUserToDatabase(registrationUserDTO);
                _logger.Log(LogLevel.Information, "Данные были сохранены в базу данных");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Исключение при сохранении в базу данных: {ex.Message}");
                throw;
            }
        }
        private string GetHashPasswrod(string password, string confirmPasswrod)
        {
            
            bool isValid = password.Equals(confirmPasswrod);
            if (isValid)
            {
                string hashPassword = BCrypt.Net.BCrypt.HashPassword(password);
                _logger.Log(LogLevel.Information, $"Был получен hashPassword: {hashPassword}");

                return hashPassword;
                
            }
            throw new Exception("Пароли не совпадают");
        }

        private async Task AddUserToDatabase(RegistrationUserDTO registrationUserDTO)
        {
            User user = _mapper.Map<User>(registrationUserDTO);
            user.UserId = Guid.NewGuid();
            user.HashPassword = GetHashPasswrod(registrationUserDTO.Password, registrationUserDTO.ConfirmPassword);
            user.RegistrationDate = DateTime.UtcNow;

            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();
        }
    }
}
