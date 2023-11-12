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

        public string GetPhoneNumberFromJwtToken(string authHeaderValue)
        {
            try
            {
                string jwtToken = authHeaderValue.ToString().Replace("Bearer ", string.Empty);
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                    JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
                    Claim? phoneNumberClaim = jwtSecurityToken?.Claims.FirstOrDefault(claim => claim.Type == "PhoneNumber");
                    if (phoneNumberClaim != null)
                    {
                        _logger.LogInformation($"Полученный номер телефона: {phoneNumberClaim.Value}");

                        return phoneNumberClaim.Value;
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось прочитать номер телефона из токена");
                    }
                }
                else
                {
                    _logger.LogWarning("Токен либо отсутствует, либо имеет неверный формат");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при извлечении номера телефона из JWT токена");
            }
            return string.Empty;
        }
    }
}
