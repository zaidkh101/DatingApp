using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            _photoService = photoService;
        }


      
        [HttpGet]

        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());
            userParams.CurrentUserName = user.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender== "male" ? "female" : "male";
            }


            var Users = await userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(Users.CurrentPage, Users.PageSize, Users.TotalCount, Users.TotalPages);


            return Ok(Users);
        }

        [HttpGet("{username}", Name = "GetUser")]

        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            var User = await userRepository.GetMemberAsync(username);

            return User;

        }


        [HttpPut]

        public async Task<ActionResult> UpdateUser(MemberUpdateDTO Model)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

            mapper.Map(Model, user);

            userRepository.Update(user);

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed To Update User");


        }


        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var Photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if (user.Photos.Count == 0)
            {
                Photo.IsMain = true;
            }

            user.Photos.Add(Photo);

            userRepository.Update(user);

            if (await userRepository.SaveAllAsync())
            {

                return CreatedAtRoute("GetUser", new { username = user.UserName }, mapper.Map<PhotoDTO>(Photo));
            }


            return BadRequest("Problems adding Photo");



        }

        [HttpPut("set-main-photo/{photoId}")]

        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(id => id.Id == photoId);

            if (photo.IsMain) return BadRequest("this is already your main photo");

            var CurrentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (CurrentMain != null)
            {
                CurrentMain.IsMain = false;
            }

            photo.IsMain = true;

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");



        }


        [HttpDelete("delete-photo/{photoId}")]

        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }

            user.Photos.Remove(photo);
            if (await userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Failed to delete the Photo");
        }
    }
}
