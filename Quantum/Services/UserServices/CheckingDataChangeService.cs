using Quantum.Interfaces.UserInterface;

namespace Quantum.Services.UserServices
{
    public class CheckingDataChangeService : ICheckingDataChange
    {
        private readonly JwtTokenProcess _jwtTokenProcess;
        private readonly ILogger<CheckingDataChangeService> _logger;
        public CheckingDataChangeService(JwtTokenProcess jwtTokenProcess, ILogger<CheckingDataChangeService> logger)
        {
            _jwtTokenProcess = jwtTokenProcess;
            _logger = logger;
        }

        // Допилить
        public bool CheckingDataChange(string authToken)
        {
            try
            {
                string phoneNumberClaim = _jwtTokenProcess.GetPhoneNumberFromJwtToken(authToken);

                Guid userId = _jwtTokenProcess.GetUserIdFromJwtToken(authToken);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
            
        }
    }
}
