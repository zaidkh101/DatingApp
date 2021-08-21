using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context;

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }


        public async Task<UserLike> GetUserLike(int SourceUserId, int LikedUserId)
        {
            return await context.Likes.FindAsync(SourceUserId, LikedUserId);
        }

        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        {
            var Users = context.Users.OrderBy(u => u.UserName).AsQueryable();
            var Likes = context.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                Likes = Likes.Where(like => like.SourceUserId == likesParams.UserId);
                Users = Likes.Select(like => like.LikedUser);
            }
            if (likesParams.Predicate == "likedBy")
            {
                Likes = Likes.Where(like => like.LikedUserId == likesParams.UserId);
                Users = Likes.Select(like => like.SourceUser);
            }

            var likedUsers = Users.Select(user => new LikeDTO
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id
            }
             );

            return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int UserId)
        {
            return await context.Users.Include(x => x.LikedUsers).FirstOrDefaultAsync(x => x.Id == UserId);
        }
    }
}
