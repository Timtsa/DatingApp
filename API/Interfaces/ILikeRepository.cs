using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikeRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<AppUser> GetUserWitchLikes(int userId);
        Task<PagedList<LikeDTO>> GetUserLikes (LikeParams likeParams);
        void UnLike (UserLike userLike);
    }
}