using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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

        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper mapper;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
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


            user.UserName = registerDTO.Username.ToLower();


            var Result = await userManager.CreateAsync(user, registerDTO.Password);

            if (!Result.Succeeded) return BadRequest();

            var RoleResult = await userManager.AddToRoleAsync(user, "Member");

            if (!RoleResult.Succeeded) return BadRequest(Result.Errors);

            return new UserDTO
            {
                Username = user.UserName,
                Token = await _tokenService.CreateTokenAsync(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var User = await userManager.Users.Include(p => p.Photos).SingleOrDefaultAsync(user => user.UserName.ToLower() == loginDTO.Username.ToLower());

            if (User == null)
            {
                return Unauthorized("Invalid Username");
            }


            var Result = await signInManager.CheckPasswordSignInAsync(User, loginDTO.Password, false);

            if (!Result.Succeeded) return Unauthorized();



            return new UserDTO
            {
                Username = User.UserName,
                Token = await _tokenService.CreateTokenAsync(User),
                PhotoUrl = User.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = User.KnownAs,
                Gender = User.Gender

            };
        }
        private async Task<Boolean> UserExists(string Username)
        {
            return await userManager.Users.AnyAsync(user => user.UserName.ToLower() == Username.ToLower());
        }

    }
}
