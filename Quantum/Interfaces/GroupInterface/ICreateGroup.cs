namespace Quantum.Interfaces.GroupInterface
{
    public interface ICreateGroup
    {
        public Task EnterInformationGroup(string name, string description);
        public Task AddParticipants(Guid authorId, Guid friendId);
    }
}
