using API.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface ITokenService
    {
        public  Task <string> CreateTokenAsync(AppUser appUser);
    }
}
