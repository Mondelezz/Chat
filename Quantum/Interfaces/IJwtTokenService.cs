namespace Quantum.Interfaces
{
    public interface IJwtTokenProcess
    {
        public string GetPhoneNumberFromJwtToken(string authHeaderValue);
    }
}
