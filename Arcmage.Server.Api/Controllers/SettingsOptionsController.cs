using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
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
                if (repository.ServiceUser != null)
                {
                    await repository.Context.Entry(repository.ServiceUser).Reference(x => x.Role).LoadAsync();
                   
                    if (repository.ServiceUser.Role.Guid == PredefinedGuids.Administrator ||
                        repository.ServiceUser.Role.Guid == PredefinedGuids.ServiceUser)
                    {
                        settingsOptions.IsPlayerAdmin = true;
                    }
                }

                settingsOptions.Roles = repository.Context.Roles.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                return Ok(settingsOptions);
            }
        }
    }
}
