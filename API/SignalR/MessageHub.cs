using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extansions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHubContext<PresenceHub> _presencs;
        private readonly PresenceTracker _tracker;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository,
        IHubContext<PresenceHub> presencs, PresenceTracker tracker)
        {
            _presencs = presencs;
            _tracker = tracker;
            _userRepository = userRepository;
            _mapper = mapper;
            _messageRepository = messageRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetuserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group=await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            var messeges = await _messageRepository.GetMessageThread(Context.User.GetuserName(), otherUser);
            await Clients.Caller.SendAsync("ReciveMassegeThread", messeges);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {

            var userName = Context.User.GetuserName();
            if (userName == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot sen message to your self");

            var sender = await _userRepository.GetUserByUsernameAsync(userName);
            var recepient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (recepient == null)
                throw new HubException("Not found");

            var message = new Message
            {
                Sender = sender,
                SenderUserName = sender.UserName,
                Recepient = recepient,
                RecepientUserName = recepient.UserName,
                Content = createMessageDto.Content
            };
            _messageRepository.AddMessage(message);

            var groupName = GetGroupName(sender.UserName, recepient.UserName);
            var group = await _messageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.UserName == recepient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else{
                var connections = await _tracker.GetConnectionsForUser(recepient.UserName);
                if(connections!=null){
                    await _presencs.Clients.Clients(connections).SendAsync("NewMessageRecived", 
                    new {userName=sender.UserName, knownAs=sender.KnownAs});

                }
            }

            if (await _messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }

            //  throw new HubException("Faild to send message");
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetuserName());
            if (group == null)
            {
                group = new Group(groupName);
                _messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _messageRepository.SaveAllAsync())
            return group;
            throw new HubException("Faile adding to group"); 
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _messageRepository.GetGroupForConnections(Context.ConnectionId);
            var connection =group.Connections.FirstOrDefault(x=>x.ConnectionId==Context.ConnectionId);

            _messageRepository.RemoveConnection(connection);
           if ( await _messageRepository.SaveAllAsync()) return group;
           throw new HubException("Faile to remove from nessage group");

        }
    }
}