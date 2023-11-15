using Quantum.Interfaces;
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
                    _logger.LogWarning("Токен либо отсутствует, либо имеет неверный формат");
                    throw new Exception("Токен либо отсутствует, либо имеет неверный формат");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при извлечении номера телефона из JWT токена {ex.Message}");
                throw;
            }
        }
        public string GetPhoneNumberFromJwtToken(string authHeaderValue)
        {
            JwtSecurityToken jwtSecurityToken = GetJwtToken(authHeaderValue);
            Claim? phoneNumberClaim = jwtSecurityToken?.Claims.First(claim => claim.Type == "PhoneNumber");
            if (phoneNumberClaim != null)
            {
                _logger.LogInformation($"Полученный номер телефона: {phoneNumberClaim.Value}");

                return phoneNumberClaim.Value;
            }
            else
            {
                _logger.LogWarning("Не удалось прочитать номер телефона из токена");
                return string.Empty;
            }
        }

        public Guid GetUserIdFromJwtToken(string authHeaderValue)
        {            
            JwtSecurityToken jwtSecurityToken = GetJwtToken(authHeaderValue);
            Claim? userIdClaim = jwtSecurityToken.Claims.First(claim => claim.Type == "Id");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                _logger.LogInformation($"Полученный userId: {userId}");

                return userId;
            }
            else
            {
                _logger.LogWarning("Не удалось прочитать номер телефона из токена");

                return Guid.Empty;
            }
        }
    }
}
