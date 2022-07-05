using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;

        public PhotoRepository(DataContext context)
        {
        _context = context;
        }

        public async Task<Photo> GetPhotoById(int Id)        
        {

            return await _context.Photos.IgnoreQueryFilters()
            .SingleOrDefaultAsync(p=>p.Id==Id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _context.Photos
            .IgnoreQueryFilters()
            .Where(p=>p.isApproved ==false)
            .Select(u=> new PhotoForApprovalDto{
               Id=u.Id,
               UserName = u.AppUser.UserName,
               Url = u.Url,
               isApproved = u.isApproved
            }).ToListAsync();
        
        }

      

        public void RemovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}