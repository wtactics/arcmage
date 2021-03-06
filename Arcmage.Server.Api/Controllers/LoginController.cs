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

                try
                {
                    var user = repository.Context.Users.SingleOrDefault(x => x.Email == login.Email);

                    if (user != null)
                    {
                        if (user.IsDisabled)
                        {
                            return BadRequest(new { message = "Login failed" });
                        }

                        if (!user.IsVerified)
                        {
                            return BadRequest(new { message = "Login failed" });
                        }

                        if (Hasher.VerifyHashedPassword(user.Password, login.Password))
                        {
                            user.Token = TokenGenerator.CreateToken(user.Guid.ToString(), TimeSpan.FromDays(7));
                            return Ok(user.Token);
                        }
                        if (user.Password == login.Password)
                        {
                            // simple password hash is not sufficient we'll  update it
                            user.Password = Hasher.HashPassword(user.Password);
                            repository.Context.SaveChanges();
                            user.Token = TokenGenerator.CreateToken(user.Guid.ToString(), TimeSpan.FromDays(7));
                            return Ok(user.Token);
                        }

                    }
                }
                catch (Exception)
                {
                    return BadRequest(new { message = "Login failed" });
                }
               
                return BadRequest(new { message = "Username or password is incorrect" });
            }
        }

     
    }
}
