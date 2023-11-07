using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        public AuthorizationService(DataContext dataContext, IMapper mapper, ILogger logger)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _logger = logger;   
        }

        public string AuthenticateUser(AuthorizationUserDTO authorizationUserDTO)
        {
            try
            {
                User? user = _dataContext.Users.FirstOrDefault(pN => pN.PhoneNumber == authorizationUserDTO.PhoneNumber);
                if (user == null)
                {
                    _logger.Log(LogLevel.Warning, "Пользователь не найден");

                    throw new Exception("Пользователь не найден");
                }
                else
                {
                    bool validatePassword = BCryptNet.Verify(authorizationUserDTO.Password, user.HashPassword);
                    if (validatePassword)
                    {
                        ClaimsIdentity identity = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimsIdentity.DefaultNameClaimType, authorizationUserDTO.Password),
                            new Claim(ClaimsIdentity.DefaultNameClaimType, authorizationUserDTO.PhoneNumber),
                            new Claim("Id", user.UserId.ToString())
                        });
                        DateTime now = DateTime.UtcNow;
                        JwtSecurityToken jwt
                    }
                }
                
            }
            catch (Exception)
            {

                throw;
            }

            
        }
        public string CreateJwtToken()
    }
}
