using API.DTOs;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IHubContext<PresenceHub> PresenceContext;
        private readonly PresenceTracker PresenceTracker;

        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository, IHubContext<PresenceHub> PresenceContext, PresenceTracker PresenceTracker)
        {
            this.messageRepository = messageRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.PresenceContext = PresenceContext;
            this.PresenceTracker = PresenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var HttpContext = Context.GetHttpContext();

            var OtherUser = HttpContext.Request.Query["user"].ToString();

            var GroupName = GetGroupName(Context.User.GetUsername(), OtherUser);

            var Group = await AddToGroup(GroupName);

            await Clients.Group(GroupName).SendAsync("UpdatedGroup", Group);

            var Messages = await messageRepository.GetMessageThread(Context.User.GetUsername(), OtherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", Messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var Group = await RemoveFromMessageGroup();
            await Clients.Group(Group.Name).SendAsync("UpdatedGroup", Group);

            await base.OnDisconnectedAsync(exception);
        }


        public async Task SendMessage(CreateMessageDTO createMessageDTO)
        {
             var username = Context.User.GetUsername();

            if (username == createMessageDTO.RecipientUsername.ToLower())
            {
                throw new HubException("you can't send messages to yourself");
            }

            var Sender = await userRepository.GetUserByUserNameAsync(username);
            var Recipient = await userRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

            if (Recipient == null)
            {
                throw new HubException("Not Found User");
            }

            var Message = new Message
            {
                Sender = Sender,
                Recipient = Recipient,
                SenderUsername = Sender.UserName,
                RecipientUsername = Recipient.UserName,
                Content = createMessageDTO.Content
            };


            var GroupName = GetGroupName(Sender.UserName, Recipient.UserName);

            var Group = await messageRepository.GetMessageGroup(GroupName);

            if (Group.Connections.Any(x => x.Username == Recipient.UserName))
            {
                Message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(Recipient.UserName);
                if (connections != null)
                {
                    await PresenceContext.Clients.Clients(connections).SendAsync("NewMessageReceived"
                        , new { username = Sender.UserName, knownAs = Sender.KnownAs });
                }
            }
            messageRepository.AddMessage(Message);

            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(GroupName).SendAsync("NewMessage", mapper.Map<MessageDTO>(Message));
            }
        }

        private async Task<Group> AddToGroup(string GroupName)
        {
            var Group = await messageRepository.GetMessageGroup(GroupName);
            var Connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (Group == null)
            {
                Group = new Group(GroupName);
                messageRepository.AddGroup(Group);
            }

            Group.Connections.Add(Connection);
            if (await messageRepository.SaveAllAsync()) return Group;

            throw new HubException("Failed to join groups");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var Group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            var Connection = Group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            messageRepository.RemoveConnection(Connection);

            if (await messageRepository.SaveAllAsync()) return Group;

            throw new HubException("Failed to Remove from group");
        }

        private string GetGroupName(string Caller, string Other)
        {
            var StringCompare = string.CompareOrdinal(Caller, Other) < 0;

            return StringCompare ? $"{Caller}-{Other}" : $"{Other}-{Caller}";
        }
    }
}
