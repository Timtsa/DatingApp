using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Extansions
{
    public static class ClaimsPrincipleExtentions
    {
        public static string GetuserName(this ClaimsPrincipal user){
           
           return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}