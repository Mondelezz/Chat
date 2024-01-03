using Quantum.Interfaces;
using Quantum.UserP.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Quantum.Services
{
    public class JwtTokenProcess : IJwtTokenProcess
    {
        private readonly ILogger<JwtTokenProcess> _logger;

        public JwtTokenProcess(ILogger<JwtTokenProcess> logger)
        {
            _logger = logger;
        }
        private JwtSecurityToken GetJwtToken(string authHeaderValue)
        {
            try
            {
                string jwtToken = authHeaderValue.ToString().Replace("Bearer ", string.Empty);
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                    JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
                    return jwtSecurityToken;
                }
                else
                {
                    _logger.LogWarning("Токен либо отсутствует, либо имеет неверный формат\n");
                    throw new Exception("Токен либо отсутствует, либо имеет неверный формат\n");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при извлечении номера телефона из JWT токена {ex.Message} \n");
                throw;
            }
        }
        public string GetPhoneNumberFromJwtToken(string authHeaderValue)
        {
            JwtSecurityToken jwtSecurityToken = GetJwtToken(authHeaderValue);
            Claim? phoneNumberClaim = jwtSecurityToken?.Claims.First(claim => claim.Type == "PhoneNumber");
            if (phoneNumberClaim != null)
            {
                _logger.LogInformation($"Полученный номер телефона из jwtToken: {phoneNumberClaim.Value}\n");

                return phoneNumberClaim.Value;
            }
            else
            {
                _logger.LogWarning("Не удалось прочитать номер телефона из токена\n");
                return string.Empty;
            }
        }

        public Guid GetUserIdFromJwtToken(string authHeaderValue)
        {            
            JwtSecurityToken jwtSecurityToken = GetJwtToken(authHeaderValue);
            Claim? userIdClaim = jwtSecurityToken.Claims.First(claim => claim.Type == "Id");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                _logger.LogInformation($"Полученный userId: {userId}\n");

                return userId;
            }
            else
            {
                _logger.LogWarning("Не удалось прочитать userId из токена\n");

                return Guid.Empty;
            }
        }
        public List<UserOpenData> GetUserInfo(string authHeaderValue)
        {
            JwtSecurityToken jwtSecurityToken = GetJwtToken(authHeaderValue);
            Claim? phoneNumberClaim = jwtSecurityToken?.Claims.First(claim => claim.Type == "PhoneNumber");
            Claim? userNameClaim = jwtSecurityToken?.Claims.First(claim => claim.Type == ClaimsIdentity.DefaultNameClaimType);
           
            if (phoneNumberClaim != null && userNameClaim != null)
            {
                UserOpenData userData = new UserOpenData()
                {
                    PhoneNumber = phoneNumberClaim.Value,
                    UserName = userNameClaim.Value
                };
                
                _logger.LogInformation($"Полученный номер телефона из jwtToken: {phoneNumberClaim.Value}\n");
                _logger.LogInformation($"Полученное имя из jwtToken: {userNameClaim.Value}\n");

                List<UserOpenData> userInfo = new List<UserOpenData>
                {
                    userData
                };

                return userInfo;
            }
            else
            {
                _logger.LogWarning("Не удалось прочитать номер телефона из токена\n");
                throw new Exception("Не удалось распознать jwt");
            }
        }
    }
}
