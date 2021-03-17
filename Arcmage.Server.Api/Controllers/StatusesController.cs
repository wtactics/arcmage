using System;
using System.Linq;
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
    [Route(Routes.Statuses)]
    public class StatusesController : ControllerBase
    {

        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository())
            {
                var query = await repository.Context.Statuses.ToListAsync();
                var result = new ResultList<Status>(query.Select(x => x.FromDal()).ToList());
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
                var result = await repository.Context.Statuses.FindByGuidAsync(id);
                return Ok(result.FromDal());
            }
        }

        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody]  Status status)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.CreateStatus))
                {
                    return Forbid("You are not allowed to create statuses.");
                }

                if (string.IsNullOrWhiteSpace(status.Name))
                {
                    return BadRequest("The name is required.");
                }
                var statusModel = repository.CreateStatus(status.Name, Guid.NewGuid());
                return Ok(statusModel.FromDal());
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Status status)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditStatus))
                {
                    return Forbid("You are not allowed to edit statuses.");
                }

                if (string.IsNullOrWhiteSpace(status.Name))
                {
                    return BadRequest("The name is required.");
                }
                var statusModel = await repository.Context.Statuses.FindByGuidAsync(id);
                statusModel.Patch(status, repository.ServiceUser);
                await repository.Context.SaveChangesAsync();
                return Ok(statusModel.FromDal());
            }
        }


    }
}
