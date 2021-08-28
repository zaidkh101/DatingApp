using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;

        public MessageRepository(DataContext context)
        {
            this.context = context;
        }


        public void AddMessage(Message message)
        {
          
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public Task<PagedList<MessageDTO>> GetMessageForUser()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MessageDTO>> GetMessageThread(int currentUserId, int recipientId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
