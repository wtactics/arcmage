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

    [Route(Routes.Series)]
    public class SeriesController : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository())
            {
                var query = await repository.Context.Series.ToListAsync();
                var result = new ResultList<Serie>(query.Select(x => x.FromDal()).ToList());
                return Ok(result);
            }
        }


        [HttpGet]
        [Route("id")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {
            using (var repository = new Repository())
            {
                var result = await repository.Context.Series.FindByGuidAsync(id);
                return Ok(result.FromDal(true));
            }
        }

        [HttpPost]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] Serie serie)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.CreateSerie))
                {
                    return Forbid("You are not allowed to create series.");
                }
                if (string.IsNullOrWhiteSpace(serie.Name))
                {
                    return BadRequest("The name is required.");
                }
                var serieModel = repository.CreateSeries(serie.Name, Guid.NewGuid());
                return Ok(serieModel.FromDal());
            }
        }

        [Authorize]
        [HttpPatch]
        [Produces("application/json")]
        [Route("{id}")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Serie serie)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditSerie))
                {
                    return Forbid("You are not allowed to edit series.");
                }
                if (string.IsNullOrWhiteSpace(serie.Name))
                {
                    return BadRequest("The name is required.");
                }
                var serieModel = await repository.Context.Series.FindByGuidAsync(id);
                serieModel.Patch(serie, repository.ServiceUser);
                await repository.Context.SaveChangesAsync();
                return Ok(serieModel.FromDal());
            }
        }

    }
}
