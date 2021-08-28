﻿using API.DTOs;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using AutoMapper;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.messageRepository = messageRepository;
            this.mapper = mapper;
        }



        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUsername();

            if (username == createMessageDTO.RecipientUsername.ToLower())
            {
                return BadRequest("you can't send messages to yourself");
            }

            var Sender = await userRepository.GetUserByUserNameAsync(username);
            var Recipient = await userRepository.GetUserByUserNameAsync(createMessageDTO.RecipientUsername);

            if (Recipient == null)
            {
                return NotFound();
            }

            var Message = new Message
            {
                Sender = Sender,
                Recipient = Recipient,
                SenderUsername = Sender.UserName,
                RecipientUsername = Recipient.UserName,
                Content = createMessageDTO.Content
            };

            messageRepository.AddMessage(Message);

            if (await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDTO>(Message));

            return BadRequest("Failed to send message");
        }
    }
}
