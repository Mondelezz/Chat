using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        private readonly JwtTokenProcess _jwtTokenProcess;
        public UserHubService(DataContext dataContext, IMapper mapper, ILogger<UserHubService> logger, JwtTokenProcess jwtTokenProcess)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _logger = logger;
            _jwtTokenProcess = jwtTokenProcess;
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
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize]
        public async Task<UserInfoOutput> UserUpdateInfoAsync(UpdateInfo updateInfo, string token)
        {
            try
            {
                if (updateInfo != null)
                {
                    Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(token);
                    User user = _dataContext.Users.First(id => id.UserId == userId);
                    if (user != null)
                    {
                        user.UserName = updateInfo.UserName;
                        user.PhoneNumber = updateInfo.PhoneNumber;
                        _dataContext.Users.Update(user);
                        await _dataContext.SaveChangesAsync();

                        _logger.Log(LogLevel.Information, $"Данные обновлены успешно.\n" +
                            $"{user.UserName}, " +
                            $"{user.PhoneNumber}\n");

                        UserInfoOutput userInfoOutput = _mapper.Map<UserInfoOutput>(user);
                        return userInfoOutput;
                    }
                    else
                    {
                        _logger.Log(LogLevel.Warning, $"Пользователь с {userId} не найден.");
                        throw new Exception("Пользователь не найден.");                       
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Warning, $"Информация о пользователе не указана.{updateInfo.UserName} \t {updateInfo.PhoneNumber}");
                    throw new Exception("Проверьте корректность введённых данных.");
                }
            }
            catch (Exception)
            {
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
