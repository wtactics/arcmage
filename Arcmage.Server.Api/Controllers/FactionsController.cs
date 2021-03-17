using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Auth;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Factions)]
    public class FactionsController : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository())
            {
                var query = await repository.Context.Factions.ToListAsync();
                var result = new ResultList<Faction>(query.Select(x => x.FromDal()).ToList());
                return Ok(result);
            }
        }


        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {
            using (var repository = new Repository())
            {
                var result = await repository.Context.Factions.FindByGuidAsync(id);
                return Ok(result.FromDal());
            }
        }

        [HttpPost]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] Faction faction)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.CreateFaction))
                {
                    return Forbid();
                }

                if (string.IsNullOrWhiteSpace(faction.Name))
                {
                    return BadRequest("The name is required.");
                }
                var factionModel = repository.CreateFaction(faction.Name, Guid.NewGuid());
                return Ok(factionModel.FromDal());
            }
        }


        [Authorize]
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Faction faction)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditFaction))
                {
                    return Forbid();
                }

                if (string.IsNullOrWhiteSpace(faction.Name))
                {
                    return BadRequest("The name is required.");
                }
                var factionModel = await repository.Context.Factions.FindByGuidAsync(id);
                factionModel.Patch(faction, repository.ServiceUser);
                await repository.Context.SaveChangesAsync();
                return Ok(factionModel.FromDal());
            }
        }

    }
}
