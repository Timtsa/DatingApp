using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }
        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
            .Include(x => x.Sender)
            .Include(x => x.Recepient)
            .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                       .OrderByDescending(m => m.MessageSent)
                       .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                       .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecepientUserName == messageParams.UserName &&
                u.RecepientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUserName == messageParams.UserName &&
                u.SenderDeleted == false),
                _ => query.Where(u => u.RecepientUserName == messageParams.UserName && u.RecepientDeleted == false
                && u.DateRead == null)
            };
          

            return await PagedList<MessageDto>.CreateAsync(query,
                messageParams.PageSize, messageParams.PageNumber);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                    .Where(m => m.RecepientUserName == currentUserName
                    && m.SenderUserName == recipientUserName && m.RecepientDeleted == false
                    || m.SenderUserName == currentUserName
                    && m.RecepientUserName == recipientUserName && m.SenderDeleted == false)
                    .OrderBy(m => m.MessageSent)
                    .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

            var unreadMessages = messages.Where(m => m.RecepientUserName == currentUserName
                                                && m.DateRead == null).ToList();
            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }               
            }
            return messages;

        }



        public async Task<Group> GetGroupForConnections(string connectionId)
        {
            return await _context.Groups
            .Include(c => c.Connections)
            .Where(c => c.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
        }
    }
}