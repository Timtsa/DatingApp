using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikeRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDTO>> GetUserLikes(LikeParams likeParams)
        {
           var users =_context.Users.OrderBy(u=>u.UserName).AsQueryable();
           var likes = _context.Likes.AsQueryable();
           var iLikes = _context.Likes.Select(l=>l.LikedUserId).ToList();

           if(likeParams.Predicate == "liked")
           {
              likes = likes.Where(like=>like.SourceUserId ==likeParams.UserId);
              users = likes.Select(like=> like.LikedUser);
           }

           if(likeParams.Predicate == "likedBy")
           {
               likes = likes.Where(like=>like.LikedUserId ==likeParams.UserId);
              users = likes.Select(like=> like.SourceUser);
           }
          var  likedUsers=  users.Select(user=>new LikeDTO{
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p=>p.IsMain).Url,
                City = user.City,  
                IsLiked = iLikes.Contains(user.Id) ? true :false              
            });
            return await PagedList<LikeDTO>.CreateAsync(likedUsers,likeParams.PageSize,
            likeParams.PageNumber);

        }

        public async Task<AppUser> GetUserWitchLikes(int userId)
        {
            return await _context.Users
               .Include(x=>x.LikedUsers)
               .FirstOrDefaultAsync(x=>x.Id ==userId);
        }

        public void UnLike(UserLike userLike)
        {
           _context.Likes.Remove(userLike);
        }
    }
}