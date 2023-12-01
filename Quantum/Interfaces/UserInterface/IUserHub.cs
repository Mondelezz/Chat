﻿using Quantum.Models;
using Quantum.Models.DTO;

namespace Quantum.Interfaces.UserInterface
{
    public interface IUserHub
    {
        public Task EnterUserInformationAsync(RegistrationUserDTO registrationUserDTO);
        public Task<UserInfoOutput> UserUpdateInfoAsync(UsersOpenData updateInfo, string token);
    }
}
