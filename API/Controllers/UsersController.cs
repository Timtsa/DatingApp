using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extansions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams )
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetuserName());
            userParams.CurrentUsername = User.GetuserName();
            if(string.IsNullOrEmpty(userParams.Gender)){
                userParams.Gender = user.Gender=="male"? "female":"male";
            }
            var users = await _userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);
            return Ok(users);
        }

        [HttpGet("{username}", Name ="GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {

            return await _userRepository.GetMemberDtoAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetuserName());
            _mapper.Map(memberUpdateDto, user);
            _userRepository.Update(user);
            if (await _userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Faild to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetuserName());

            var result = _photoService.AddPhotoAsync(file);

            if (result.Result.Error!=null) return BadRequest(result.Result.Error);

            var photo = new Photo
            {
                Url =result.Result.SecureUrl.AbsoluteUri,
                PublicId = result.Result.PublicId                
            };
            if (user.Photos.Count==0){
                photo.IsMain=true;
            }
            user.Photos.Add(photo);
            
            if(await _userRepository.SaveAllAsync()){
                return CreatedAtRoute("GetUser",new{username=user.UserName}, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){

            var user = await _userRepository.GetUserByUsernameAsync(User.GetuserName());

            var photo = user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo.IsMain) return BadRequest("Photo is already your main");

            var currentMain = user.Photos.First(x =>x.IsMain);
            if(currentMain!=null)  currentMain.IsMain=false;
            photo.IsMain=true;
            if(await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed set photo to main");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId){

            var user = await _userRepository.GetUserByUsernameAsync(User.GetuserName());
            var photo = user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if (photo ==null) return NotFound();

            if(photo.IsMain) return BadRequest ("You can not delete main photo");

            if(photo.PublicId!=null){
            var result = _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Result.Error !=null) return BadRequest(result.Result.Error.Message);
            }
            user.Photos.Remove(photo);
            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Error during saving to database");



        }

    }
}