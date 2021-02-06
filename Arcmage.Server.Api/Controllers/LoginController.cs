using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Arcmage.Configuration;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Model;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Login)]
    public class LoginController : ControllerBase
    {

        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        public IActionResult Post([FromBody] Login login)
        {

            if (login == null) return BadRequest("No login info");

            using (var repository = new Repository())
            {
              
                var user = repository.Context.Users.SingleOrDefault(x => x.Email == login.Email);
              
                if (user  != null)
                {
                   
                    if (Hasher.VerifyHashedPassword(user.Password, login.Password))
                    {
                        user.Token = CreateToken(user);
                        return Ok(user.Token);
                    }
                    if (user.Password == login.Password)
                    {
                        // simple password hash is not sufficient we'll  update it
                        user.Password = Hasher.HashPassword(user.Password);
                        repository.Context.SaveChanges();
                        user.Token = CreateToken(user);
                        return Ok(user.Token);
                    }

                }
                return BadRequest(new { message = "Username or password is incorrect" }); ;
            }
        }

        private static string CreateToken(UserModel user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Settings.Current.TokenEncryptionKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Guid.ToString()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
