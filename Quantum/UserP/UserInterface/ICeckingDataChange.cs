namespace Quantum.UserP.UserInterface
{
    public interface ICheckingDataChange
    {
        public Task<bool> CheckingDataChangeAsync(string authToken, string senderPhoneNumber);
    }
}
