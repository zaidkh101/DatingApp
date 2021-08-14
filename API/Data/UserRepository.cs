using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MemberDTO> GetMemberAsync(string Username)
        {
            return await _context.Users.Where(x => x.UserName == Username).ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
        {
            var Query = _context.Users.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider).AsNoTracking();

            return await PagedList<MemberDTO>.CreateAsync(Query, userParams.PageNumber, userParams.PageSize);

        }

        public async Task<AppUser> GetUserByIdAsync(int Id)
        {
            return await _context.Users.FindAsync(Id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string Username)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == Username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser User)
        {
            _context.Entry(User).State = EntityState.Modified;
        }
    }
}
