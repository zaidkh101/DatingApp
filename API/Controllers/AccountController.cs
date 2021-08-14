using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
        private readonly IMapper mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            this.mapper = mapper;
        }


        [HttpPost("Register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {

            if (await UserExists(registerDTO.Username))
            {
                return BadRequest("Username is Taken");
            }

            var user = mapper.Map<AppUser>(registerDTO);

            using var Hmac = new HMACSHA512();

            user.UserName = registerDTO.Username.ToLower();
            user.PasswordHash = Hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = Hmac.Key;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTO
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
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
                PhotoUrl = User.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = User.KnownAs
            };
        }
        private async Task<Boolean> UserExists(string Username)
        {
            return await _context.Users.AnyAsync(user => user.UserName == Username.ToLower());
        }

    }
}
