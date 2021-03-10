using System;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Configuration;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Users)]
    public class UsersController : ControllerBase
    {
        public ISendGridClient SendGridClient { get; set; }

        public UsersController(ISendGridClient sendGridClient)
        {
            SendGridClient = sendGridClient;
        }

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

                if (id == "me")
                {
                    var result = repository.ServiceUser.FromDal();
                    return Ok(result);
                }

                if (repository.ServiceUser.Role.Guid == PredefinedGuids.Administrator ||
                    repository.ServiceUser.Role.Guid == PredefinedGuids.ServiceUser)
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        var userModel = await repository.Context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Guid == guid);
                        var result = userModel.FromDal(true);
                        return Ok(result);
                    }
                }

                return NotFound("User not found");

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

                if (string.IsNullOrWhiteSpace(Settings.Current.SendGridApiKey))
                {
                    // We're not using email validation, so just set it on verified
                    userModel.IsVerified = true;
                    await repository.Context.SaveChangesAsync();
                }
                else
                {
                    // send email verification
                    await SendVerificationEmail(userModel);
                }
                return Ok(userModel.FromDal());
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] User user)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }
                var userModel = await repository.Context.Users.Include(x=>x.Role).FirstOrDefaultAsync(x=>x.Guid == id);
                if (repository.ServiceUser == null)
                {
                    return BadRequest("User not found.");
                }

                await repository.Context.Entry(repository.ServiceUser).Reference(x => x.Role).LoadAsync();

                // Only Administrators or the service user can change the role, the verification and disabled state
                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator &&
                    repository.ServiceUser.Role.Guid != PredefinedGuids.ServiceUser)
                {
                    user.IsVerified = userModel.IsVerified;
                    user.IsDisabled = userModel.IsDisabled;
                    user.Role = null;
                }

                var newRole = await repository.Context.Roles.FirstOrDefaultAsync(x => user.Role != null && x.Guid == user.Role.Guid);

                userModel.Patch(user, newRole);
                await repository.Context.SaveChangesAsync();
                return Ok(userModel.FromDal(true));
            }
        }



        [HttpPost]
        [Route("request/email/verify")]
        [Produces("application/json")]
        public async Task<IActionResult> RequestValidation([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(Settings.Current.SendGridApiKey))
            {
                return BadRequest("Email validation is disabled.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest("The email is required.");
            }
            using (var repository = new Repository())
            {
                var userModel = repository.Context.Users.FirstOrDefault(x => x.Email == user.Email);
                if (userModel == null) return BadRequest("Email not found");

                // send email verification
                await SendVerificationEmail(userModel);
                return Ok();
            }
        }

        [HttpPost]
        [Route("request/password/reset")]
        [Produces("application/json")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(Settings.Current.SendGridApiKey))
            {
                return BadRequest("Password reset is disabled.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest("The email is required.");
            }
            using (var repository = new Repository())
            {
                var userModel = repository.Context.Users.FirstOrDefault(x => x.Email == user.Email);
                if (userModel == null) return BadRequest("Email not found");
                if (!userModel.IsVerified) return BadRequest("Email not verified");

                // Send email password reset
                await SendPasswordResetEmail(userModel);
                return Ok();
            }
        }

        [Authorize]
        [HttpPost]
        [Route("password/reset")]
        [Produces("application/json")]
        public async Task<IActionResult> ResetPassword([FromBody] User user)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                if (repository.ServiceUser.Guid != user.Guid)
                {
                    return BadRequest("Not allowed to set a password for a different user'");
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

                var passwordHash = Hasher.HashPassword(user.Password);
                repository.ServiceUser.Password = passwordHash;
                repository.ServiceUser.LastLoginTime = DateTime.Now;
                await repository.Context.SaveChangesAsync();

                var result = repository.ServiceUser.FromDal();
                result.Token = TokenGenerator.CreateToken(result.Guid.ToString(), TimeSpan.FromDays(7));
                return Ok(result);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("email/verify")]
        [Produces("application/json")]
        public async Task<IActionResult> Verify()
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                repository.ServiceUser.IsVerified = true;
                await repository.Context.SaveChangesAsync();
                var result = repository.ServiceUser.FromDal();
                result.Token = TokenGenerator.CreateToken(result.Guid.ToString(), TimeSpan.FromDays(7));
                return Ok(result);
            }
        }


        private async Task<Response> SendVerificationEmail(UserModel user)
        {
            var confirmUrl = Settings.Current.PortalUrl + "/#/confirm?token=" + TokenGenerator.CreateToken(user.Guid.ToString(), TimeSpan.FromDays(1));

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("admin@arcmage.org", "Arcmage"),
                Subject = "Welcome to Arcmage! Confirm Your Email",

                PlainTextContent = 
                    "Almost there, let's confirm your email address.\n\n" +
                    "By clicking on the following link, you are confirming your email address.\n\n" +
                    confirmUrl,

                HtmlContent = "<h1>Almost there, let's confirm your email address.</h1>" +
                              "<p>By clicking on the following link, you are confirming your email address.</p>" +
                              $"<p><a clicktracking=off href='{confirmUrl}'>Confirm Email Address<a></p>",
            };
            msg.AddTo(new EmailAddress(user.Email, user.Name));
            return await SendGridClient.SendEmailAsync(msg);
        }

        private async Task<Response> SendPasswordResetEmail(UserModel user)
        {
            var resetUrl = Settings.Current.PortalUrl + "/#/password-reset?token=" + TokenGenerator.CreateToken(user.Guid.ToString(), TimeSpan.FromDays(1));

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("admin@arcmage.org", "Arcmage"),
                Subject = "Arcmage Password Reset.",

                PlainTextContent =
                    "A password reset has been requested, if that wasn't you, simply ignore this mail.\n\n" +
                    "The following link is valid for 24h and will take you to the reset page.\n\n" +
                    resetUrl,

                HtmlContent = "<h1>A password reset has been requested, if that wasn't you, simply ignore this mail.</h1>" +
                              "<p>The following link is valid for 24h and will take you to the reset page.</p>" +
                              $"<p><a  clicktracking=off href='{resetUrl}'>Reset Password<a></p>",
            };
            msg.AddTo(new EmailAddress(user.Email, user.Name));
            return await SendGridClient.SendEmailAsync(msg);
        }
    }
}
