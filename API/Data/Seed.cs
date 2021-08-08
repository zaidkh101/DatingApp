using API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            if (await context.Users.AnyAsync()) return;
            var UserData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var Users = JsonSerializer.Deserialize<List<AppUser>>(UserData);
            foreach (var User in Users)
            {
                using var hmac = new HMACSHA512();
                User.UserName = User.UserName.ToLower();
                User.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                User.PasswordSalt = hmac.Key;

                context.Users.Add(User);
            }

            await context.SaveChangesAsync();
        }
    }
}
