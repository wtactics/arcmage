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
    [Route(Routes.CardTypes)]
    public class CardTypesController : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository())
            {
                var query = await repository.Context.CardTypes.ToListAsync();
                var result = new ResultList<CardType>(query.Select(x => x.FromDal()).ToList());
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
                var result = await repository.Context.CardTypes.FindByGuidAsync(id);
                return Ok(result.FromDal());
            }
        }


        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] CardType cardType)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {

                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.CreateCardType))
                {
                    return Forbid();
                }

                if (string.IsNullOrWhiteSpace(cardType.Name))
                {
                    return BadRequest( "The name is required.");
                }
                var templateInfoModel = await repository.Context.TemplateInfoModels.FindByGuidAsync(cardType.TemplateInfo.Guid);

                var cardTypeModel = repository.CreateCardType(cardType.Name, Guid.NewGuid(), templateInfoModel);
                return Ok(cardTypeModel.FromDal());
            }
        }


        [Authorize]
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] CardType cardType)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditCardType))
                {
                    return Forbid();
                }

                if (string.IsNullOrWhiteSpace(cardType.Name))
                {
                    return BadRequest("The name is required.");
                }
                var cardTypeModel = await repository.Context.CardTypes.FindByGuidAsync(id);
                var templateInfoModel = await repository.Context.TemplateInfoModels.FindByGuidAsync(cardType.TemplateInfo.Guid);

                cardTypeModel.Patch(cardType, templateInfoModel, repository.ServiceUser);
                await repository.Context.SaveChangesAsync();
                return Ok(cardTypeModel.FromDal());
            }
        }

    }
}
