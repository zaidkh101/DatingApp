using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {

            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await context.Groups.Include(c => c.Connections).Where(c => c.Connections.Any(x => x.ConnectionId == connectionId)).FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.Include(u => u.Sender).Include(u => u.Recipient).SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDTO>> GetMessageForUser(MessageParams messageParams)
        {
            var Query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
            Query = messageParams.Container switch
            {
                "Inbox" => Query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => Query.Where(u => u.Sender.UserName == messageParams.Username & u.SenderDeleted == false),
                _ => Query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            var Messages = Query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);

            return await PagedList<MessageDTO>.CreateAsync(Messages, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var Messages = await context.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos).Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                && m.Sender.UserName == recipientUsername
                || m.Recipient.UserName == recipientUsername
                && m.Sender.UserName == currentUsername && m.SenderDeleted == false

            ).OrderBy(m => m.MessageSent).ToListAsync();

            var UnReadMessages = Messages.Where(m => m.DateRead == null
            && m.Recipient.UserName == recipientUsername).ToList();

            if (UnReadMessages.Any())
            {
                foreach (var message in UnReadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDTO>>(Messages);
        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
