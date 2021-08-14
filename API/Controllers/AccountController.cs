using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;

        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }


        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {

            if (await UserExists(registerDTO.Username))
            {
                return BadRequest("Username is Taken");
            }

            using var Hmac = new HMACSHA512();

            var User = new AppUser
            {
                UserName = registerDTO.Username.ToLower(),
                PasswordHash = Hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = Hmac.Key
            };

            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return new UserDTO
            {
                Username = User.UserName,
                Token = _tokenService.CreateToken(User)
            };
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var User = await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(user => user.UserName == loginDTO.Username.ToLower());

            if (User == null)
            {
                return Unauthorized("Invalid Username");
            }

            using var Hmac = new HMACSHA512(User.PasswordSalt);

            var ComputedHash = Hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for (int i = 0; i < ComputedHash.Length; i++)
            {
                if (ComputedHash[i] != User.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }

            return new UserDTO
            {
                Username = User.UserName,
                Token = _tokenService.CreateToken(User),
                PhotoUrl = User.Photos.FirstOrDefault(x => x.IsMain)?.Url
            };
        }
        private async Task<Boolean> UserExists(string Username)
        {
            return await _context.Users.AnyAsync(user => user.UserName == Username.ToLower());
        }

    }
}
