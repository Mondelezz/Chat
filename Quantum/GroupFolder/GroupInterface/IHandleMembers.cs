using Quantum.GroupFolder.Models;

namespace Quantum.GroupFolder.GroupInterface
{
    public interface IHandleMembers
    {
        /// <summary>
        /// Приглашение участников в открытую и закрытую группу (только друзья)
        /// </summary>
        /// <param name="groupId"></param>
        /// Айди группы
        /// <param name="senderId">
        /// Айди отправителя (кто приглашает)
        /// </param>  
        /// <param name="receiverId">
        /// Айди получателя (приглашённый)
        /// </param>
        /// <returns> Отправлена/ Не отправлена заявка </returns> 
        public Task<bool> AddMembersAsync(Guid groupId, Guid senderId, Guid receiverId);


        /// <summary>
        /// Отправка заявки в закрытую группу
        /// </summary>
        /// <param name="groupId">
        /// Айди группы
        /// </param>
        /// <param name="senderId">
        /// Айди отправителя заявки
        /// </param>
        /// <returns> Отправлена/ Не отправлена заявка </returns> 
        public Task<bool> SendRequestClosedGroup(Guid groupId, Guid senderId);


        /// <summary>
        /// Отправка заявки в открытую группу
        /// </summary>
        /// <param name="groupId">
        /// Айди группы
        /// </param>
        /// <param name="senderId">
        /// Айди отправителя заявки
        /// </param>
        /// <returns> Вступил/ Не встуипл </returns> 
        public Task<bool> SendRequestOpenGroup(Guid groupId, Guid senderId);


        /// <summary>
        /// Добалвение владельца в группу, как первого участника + назначение роли
        /// </summary>
        /// <param name="groupId">
        /// Айди группы
        /// </param>
        /// <param name="creatorId">
        /// Айди создателя
        /// </param>
        /// <returns>Добавление владельца</returns>
        public GroupUserRole AddCreator(Guid groupId, Guid creatorId);
    }
}
