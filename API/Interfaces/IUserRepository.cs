using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task <bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser> GetUserAsync(int id);

        Task<AppUser> GetUserByUsernameAsync(string name);

        Task<IEnumerable<MemberDto>> GetMembersAsync();

        Task<MemberDto> GetMemberDtoAsync(string name);

    }
}