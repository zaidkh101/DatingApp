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

        public async Task<PagedList<MessageDTO>> GetMessageForUser(MessageParams messageParams)
        {
            var Query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
            Query = messageParams.Container switch
            {
                "Inbox" => Query.Where(u => u.Recipient.UserName == messageParams.Username),
                "Outbox" => Query.Where(u => u.Sender.UserName == messageParams.Username),
                _ => Query.Where(u => u.Recipient.UserName == messageParams.Username && u.DateRead == null)
            };

            var Messages = Query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);

            return await PagedList<MessageDTO>.CreateAsync(Messages, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var Messages = await context.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos).Where(m => m.Recipient.UserName == currentUsername
                && m.Sender.UserName == recipientUsername
                || m.Recipient.UserName == recipientUsername
                && m.Sender.UserName == currentUsername

            ).OrderBy(m => m.MessageSent).ToListAsync();

            var UnReadMessages = Messages.Where(m => m.DateRead == null
            && m.Recipient.UserName == recipientUsername).ToList();

            if (UnReadMessages.Any())
            {
                foreach (var message in UnReadMessages)
                {
                    message.DateRead = DateTime.Now;
                }

                await context.SaveChangesAsync();
            }

            return mapper.Map<IEnumerable<MessageDTO>>(Messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
