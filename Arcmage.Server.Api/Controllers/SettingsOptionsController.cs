using System.Linq;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Auth;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.SettingsOptions)]
    public class SettingsOptionsController : ControllerBase
    {

        [Authorize]
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
              
                var settingsOptions = new SettingsOptions();
                settingsOptions.IsPlayerAdmin = AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditPlayer);
                settingsOptions.Roles = repository.Context.Roles.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                return Ok(settingsOptions);
            }
        }
    }
}
