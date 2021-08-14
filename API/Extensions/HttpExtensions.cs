using Microsoft.AspNetCore.Http;
using API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse Response, int CurrentPage, int ItemsPerPage, int TotalItems, int TotalPages)
        {
            var PaginationHeader = new PaginationHeader(CurrentPage, ItemsPerPage, TotalItems, TotalPages);

            var JSONOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            Response.Headers.Add("Pagination", JsonSerializer.Serialize(PaginationHeader, JSONOptions));
            Response.Headers.Add("Access-Control-Expose-Headers", "Pagination");

        }
    }
}
