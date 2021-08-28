using API.DTOs;
using API.Entities;
using API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);

        Task<PagedList<MessageDTO>> GetMessageForUser();
        Task<IEnumerable<MessageDTO>> GetMessageThread(int currentUserId, int recipientId);

        Task<bool> SaveAllAsync();

    }
}
