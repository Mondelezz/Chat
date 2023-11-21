﻿using AutoMapper;
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
        public async Task EnterUserInformationAsync(RegistrationUserDTO registrationUserDTO)
        {
            if (registrationUserDTO == null)
            {
                _logger.Log(LogLevel.Warning, "Введённые данные отсутствуют");
                return;
            }
            try
            {
                await AddUserToDatabaseAsync(registrationUserDTO);
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
                Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(token);
                User user = _dataContext.Users.First(id => id.UserId == userId);

                user.UserName = updateInfo.UserName;
                user.PhoneNumber = updateInfo.PhoneNumber;
                _dataContext.Users.Update(user);
                await _dataContext.SaveChangesAsync();

                _logger.Log(LogLevel.Information, $"Данные успешно обновлены.\n\t" +
                    $"{user.UserName},\n" +
                    $"\t{user.PhoneNumber}\n");

                UserInfoOutput userInfoOutput = _mapper.Map<UserInfoOutput>(user);
                return userInfoOutput;
           
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

        private async Task AddUserToDatabaseAsync(RegistrationUserDTO registrationUserDTO)
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
