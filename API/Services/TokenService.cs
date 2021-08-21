using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser appUser)
        {
            var Claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId,appUser.Id.ToString())
                ,
                new Claim(JwtRegisteredClaimNames.UniqueName,appUser.UserName)

            };
            var Creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(Claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = Creds
            };

            var TokenHandler = new JwtSecurityTokenHandler();

            var Token = TokenHandler.CreateToken(TokenDescriptor);

            return TokenHandler.WriteToken(Token);

        }
    }
}
