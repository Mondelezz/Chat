using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quantum.Data;
using Quantum.Interfaces;
using Quantum.Models;
using Quantum.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCryptNet = BCrypt.Net.BCrypt;
namespace Quantum.Services
{
    public class AuthorizationService : IAuthorization
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<AuthorizationService> _logger;
        public AuthorizationService(DataContext dataContext, ILogger<AuthorizationService> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<string> AuthenticateUser(AuthorizationUserDTO authorizationUserDTO)
        {
            try
            {
                User? user = await _dataContext.Users.FirstOrDefaultAsync(pN => pN.PhoneNumber == authorizationUserDTO.PhoneNumber);
                if (user == null)
                {
                    _logger.Log(LogLevel.Error, "Пользователь не найден");

                    throw new Exception("Пользователь не найден");
                }
                else
                {
                    return GetJwtToken(user, authorizationUserDTO);
                }

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Исключение при поиске пользователя в базе данных по номеру телефона: {ex.Message}");
                throw;
            }


        }
        private string GetJwtToken(User user, AuthorizationUserDTO authorizationUserDTO)
        {
            bool validatePassword = BCryptNet.Verify(authorizationUserDTO.Password, user.HashPassword);
            if (validatePassword)
            {
                ClaimsIdentity identity = new ClaimsIdentity(new[]
                {
                     new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                     new Claim(ClaimsIdentity.DefaultNameClaimType, user.PhoneNumber),
                     new Claim("Id", user.UserId.ToString())
                });
                DateTime now = DateTime.UtcNow;
                JwtSecurityToken jwt = new JwtSecurityToken(
                    issuer: JwtOptions.ISSUER,
                    audience: JwtOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(JwtOptions.KEYLIFE)),
                    signingCredentials: new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                return encodedJwt;
            }
            else
            {
                _logger.Log(LogLevel.Error, "Пользователь не найден. Неверный логин или пароль!");
                throw new Exception("Пользователь не найден. Неверный логин или пароль!");                
            }
        }
    }
}
