﻿namespace Quantum.Group.GroupInterface
{
    public interface ICreateGroup
    {
        public Task CreateGroup(string nameGroup, string descriptionGroup);
        public Task AddParticipants(Guid authorId, Guid friendId);
    }
}
