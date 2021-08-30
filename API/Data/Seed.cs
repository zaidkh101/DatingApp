using API.Entities;
using Microsoft.AspNetCore.Identity;
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
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;
            var UserData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var Users = JsonSerializer.Deserialize<List<AppUser>>(UserData);

            var Roles = new List<AppRole>
            {
                new AppRole
                {
                    Name ="Member"
                },
                new AppRole
                {
                    Name ="Admin"
                },
                new AppRole
                {
                    Name ="Moderator"
                },
            };

            foreach (var Role in Roles)
            {
                await roleManager.CreateAsync(Role);
            }


            foreach (var User in Users)
            {
                User.UserName = User.UserName.ToLower();


                await userManager.CreateAsync(User, "Pa$$w0rd");

                await userManager.AddToRoleAsync(User, "Member");
            }

            var Admin = new AppUser
            {
                UserName = "admin"
            };

            await userManager.CreateAsync(Admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(Admin, new[] { "Admin", "Moderator" });


        }
    }
}
