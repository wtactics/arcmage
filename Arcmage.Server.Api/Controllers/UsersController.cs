using System;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Configuration;
using Arcmage.DAL;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Users)]
    public class UsersController : ControllerBase
    {

        [Authorize]
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string id)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                if (id != "me")
                {
                    return BadRequest("Only 'me' is supported as the id");
                }

                var result = repository.ServiceUser.FromDal();

                return Ok(result);
            }
        }


        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                return BadRequest( "The name is required.");
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest("The email is required.");
            }
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("The password is required.");
            }
            if (string.IsNullOrWhiteSpace(user.Password2))
            {
                return BadRequest("The confirm password is required.");
            }
            if (user.Password != user.Password2)
            {
                return BadRequest("The passwords do not match.");
            }

            using (var repository = new Repository())
            {
                var passwordHash = Hasher.HashPassword(user.Password);
                var newUser = repository.Context.Users.FirstOrDefault(x => x.Email == user.Email);
                if (newUser != null) return BadRequest("Email is already taken");
                var userModel = repository.CreateUser(user.Name, user.Email, passwordHash, Guid.NewGuid());
                return Ok(userModel.FromDal());
            }
        }

    }
}
