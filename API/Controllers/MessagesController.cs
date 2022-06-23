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
      
        private readonly IUnitOfWork _unitOfWork;
       private readonly IMapper _mapper;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
           _mapper = mapper;
        }

       

        [HttpPost]
        public async Task<ActionResult<MessageDto>>CreateMessage(CreateMessageDto createMessageDto)
        {
              var userName = User.GetuserName();
              if(userName == createMessageDto.RecipientUsername.ToLower())
              return BadRequest("You cannot sen message to your self");

              var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(userName);
              var recepient = await _unitOfWork.UserRepository
              .GetUserByUsernameAsync(createMessageDto.RecipientUsername);
              if(recepient==null)
              return NotFound();

              var message = new Message{
                  Sender = sender,
                  SenderUserName =sender.UserName,
                  Recepient =recepient,
                  RecepientUserName = recepient.UserName,
                  Content = createMessageDto.Content
              };
              _unitOfWork.MessageRepository.AddMessage(message);

              if(await _unitOfWork.Complete())
              return Ok(_mapper.Map<MessageDto>(message));

              return BadRequest("Faild to send message");
        }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUsers([FromQuery]
             MessageParams messageParams)
             {
                  messageParams.UserName =User.GetuserName();
                  var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
                  Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,
                  messages.TotalCount,messages.TotalPages);

                  return messages;
             }

            
            
            [HttpDelete("{id}")]
            public async Task<ActionResult> DeleteMessage(int id){
             var userName = User.GetuserName();
             
             var message = await _unitOfWork.MessageRepository.GetMessage(id);
             if(message.Sender.UserName !=userName && message.Recepient.UserName !=userName) return Unauthorized();

             if(message.Sender.UserName==userName) message.SenderDeleted=true;
             if(message.Recepient.UserName ==userName) message.RecepientDeleted =true;

             if (message.SenderDeleted && message.RecepientDeleted) 
             _unitOfWork.MessageRepository.DeleteMessage(message);

             if(await _unitOfWork.Complete()) return Ok();
             return BadRequest("Failin during delating message");
            }

    }
}