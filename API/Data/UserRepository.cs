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
using System.Linq;


namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDto> GetMemberDtoAsync(string name, bool IsCurrentUser)
        {
            var query = _context.Users
                   .Where(x => x.UserName == name)
                   .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                   .AsQueryable();
            if (IsCurrentUser) query = query.IgnoreQueryFilters();
            var user = await query.FirstOrDefaultAsync();
            var likes = _context.Likes.Select(l => l.LikedUserId).ToList();
            if (likes.Contains(user.Id))
                user.IsLiked = true;
            return user;
        }

        public List<int> Likes()
        {
            return _context.Likes.Select(l => l.LikedUserId).ToList();
        }

          public async Task<AppUser> GetUserByPhotoId(int photoId)
        {
        return await _context.Users
        .Include(p=>p.Photos)
        .Where(p=>p.Photos.Any(x=>x.Id==photoId))
        .FirstOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var likes = _context.Likes.Select(l => l.LikedUserId).ToList();
            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };
            var queryToReturn = query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            ;

            return await PagedList<MemberDto>.CreateAsync(queryToReturn
            , userParams.PageSize, userParams.PageNumber);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string name)
        {
            return await _context.Users.Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == name);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(x => x.UserName == username)
            .Select(x => x.Gender).FirstAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }


    }
}