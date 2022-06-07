using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Extansions;
using API.Entities;
using AutoMapper;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository,
         IMessageRepository messageRepository, IMapper mapper)
        {
            _mapper = mapper;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

       

        [HttpPost]
        public async Task<ActionResult<MessageDto>>CreateMessage(CreateMessageDto createMessageDto)
        {
              var userName = User.GetuserName();
              if(userName == createMessageDto.RecipientUsername.ToLower())
              return BadRequest("You cannot sen message to your self");

              var sender = await _userRepository.GetUserByUsernameAsync(userName);
              var recepient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
              if(recepient==null)
              return NotFound();

              var message = new Message{
                  Sender = sender,
                  SenderUserName =sender.UserName,
                  Recepient =recepient,
                  RecepientUserName = recepient.UserName,
                  Content = createMessageDto.Content
              };
              _messageRepository.AddMessage(message);

              if(await _messageRepository.SaveAllAsync())
              return Ok(_mapper.Map<MessageDto>(message));

              return BadRequest("Faild to send message");
        }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUsers([FromQuery]
             MessageParams messageParams)
             {
                  messageParams.UserName =User.GetuserName();

                  var messages = await _messageRepository.GetMessagesForUser(messageParams);
                  Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,
                  messages.TotalCount,messages.TotalPages);

                  return messages;
             }

             [HttpGet("thread/{userName}")]
             public async Task<ActionResult<IEnumerable<MemberDto>>> GetMessageThread(string userName)
             {
                 var currentUserName = User.GetuserName();
                 return Ok(await _messageRepository.GetMessageThread(currentUserName,userName));
             }
            
            [HttpDelete("{id}")]
            public async Task<ActionResult> DeleteMessage(int id){
             var userName = User.GetuserName();
             
             var message = await _messageRepository.GetMessage(id);
             if(message.Sender.UserName !=userName && message.Recepient.UserName !=userName) return Unauthorized();

             if(message.Sender.UserName==userName) message.SenderDeleted=true;
             if(message.Recepient.UserName ==userName) message.RecepientDeleted =true;

             if (message.SenderDeleted && message.RecepientDeleted) 
             _messageRepository.DeleteMessage(message);

             if(await _messageRepository.SaveAllAsync()) return Ok();
             return BadRequest("Failin during delating message");
            }

    }
}