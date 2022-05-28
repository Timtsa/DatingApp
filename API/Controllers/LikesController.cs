using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extansions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
       
        private readonly IUserRepository _userRepository;
        private readonly ILikeRepository _likeRepository;

        public LikesController( IUserRepository userRepository, ILikeRepository likeRepository)
        {
            _likeRepository = likeRepository;
            _userRepository = userRepository;
           
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)  {
            
            var sourceUserId = User.GetuserId();
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _likeRepository.GetUserWitchLikes(sourceUserId);

            if(likedUser ==null) return NotFound();
            if(sourceUser.UserName ==username) return BadRequest("You cannot like yourself");

            var userLike = await _likeRepository.GetUserLike(sourceUserId,likedUser.Id);
            if (userLike !=null) return BadRequest("You alredy like this user");

            userLike = new UserLike{
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Faild to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] LikeParams likeParams){

            likeParams.UserId=User.GetuserId();
            var users= await _likeRepository.GetUserLikes(likeParams);
            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }
    }
}