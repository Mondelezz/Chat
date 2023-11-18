using Microsoft.EntityFrameworkCore;
using Quantum.Data;
using Quantum.Interfaces.UserInterface;
using Quantum.Models;

namespace Quantum.Services.UserServices
{
    public class CheckingDataChangeService : ICheckingDataChange
    {
        private readonly JwtTokenProcess _jwtTokenProcess;
        private readonly ILogger<CheckingDataChangeService> _logger;
        private readonly DataContext _dataContext;
        public CheckingDataChangeService(JwtTokenProcess jwtTokenProcess, ILogger<CheckingDataChangeService> logger , DataContext dataContext)
        {
            _jwtTokenProcess = jwtTokenProcess;
            _logger = logger;
            _dataContext = dataContext;
        }

        // Допилить
        public async Task<bool> CheckingDataChangeAsync(string authToken, string senderPhoneNumber)
        {
            try
            {               

                Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(authToken);

                User user = await _dataContext.Users.AsNoTracking().FirstAsync(x => x.UserId == userId);

                if (user.PhoneNumber == senderPhoneNumber)
                {
                    _logger.Log(LogLevel.Information, "Данные пользователя соответствуют. Изменений не выявлено.\n");
                    return false;
                }
                else
                {
                    _logger.Log(LogLevel.Warning, "Данные не соответствуют, либо пользователя не существует\nРазрываю веб-сокет соединение.\n");
                    return true;
                }
                
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Ошибка работы сервера: {ex.Message}\n");
                throw;
            }
            
        }
    }
}
