namespace Quantum.Interfaces.UserInterface
{
    public interface ICheckingDataChange
    {
        public Task<bool> CheckingDataChangeAsync(string authToken, string senderPhoneNumber);
    }
}
