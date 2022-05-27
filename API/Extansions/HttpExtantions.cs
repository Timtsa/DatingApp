using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Helpers;
using System.Text.Json;

namespace API.Extansions
{
    public static class HttpExtantions
    {
        static public void AddPaginationHeader(this HttpResponse response,
         int currentPage,int itemsPerPage,int totalItems, int totalPages){
             
             var paginationHeader = new PaginationHeader(currentPage,itemsPerPage,totalItems,totalPages);
             var options = new JsonSerializerOptions{
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase
             };
             response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader,options));
             response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}