using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Layout;
using Arcmage.Server.Api.Utils;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Arcmage.Server.Api.Controllers
{
  
    [Route(Routes.Rulings)]
    public class RulingsController : ControllerBase
    {

        [Authorize]
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {

            using (var repository = new Repository())
            {

                var rulingModel = await repository.Context.Rulings
                    .Include(x => x.Card)
                    .Include(x => x.Creator)
                    .Include(x => x.LastModifiedBy)
                    .Where(x => x.Guid == id).FirstOrDefaultAsync();
                if (rulingModel == null)
                {
                    return NotFound();
                }
                return Ok(rulingModel.FromDal());
            }
        }


        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] Ruling ruling)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator && repository.ServiceUser.Guid != PredefinedGuids.ServiceUser)
                {
                    return Forbid("You are not allowed to create rulings.");
                }

                if (ruling.Card == null)
                {
                    return BadRequest("The card is required.");
                }

                if (string.IsNullOrWhiteSpace(ruling.RuleText))
                {
                    return BadRequest("The rule text is required.");
                }

                var cardModel = await repository.Context.Cards.FindByGuidAsync(ruling.Card.Guid);
                if (cardModel == null)
                {
                    return BadRequest("The card is not found.");
                }

                var ruleModel = repository.CreateRuling(cardModel, ruling.RuleText, Guid.NewGuid());
                await repository.Context.SaveChangesAsync();
            
                return Ok(ruleModel.FromDal());
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(Guid id)
        {

            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator &&
                    repository.ServiceUser.Guid != PredefinedGuids.ServiceUser)
                {
                    return Forbid("You are not allowed to delete rulings.");
                }

                var rulingModel = await repository.Context.Rulings.Where(x => x.Guid == id).FirstOrDefaultAsync();
                if (rulingModel == null)
                {
                    return NotFound();
                }
                repository.Context.Rulings.Remove(rulingModel);
                await repository.Context.SaveChangesAsync();
                return Ok();
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Ruling ruling)
        {

            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator && repository.ServiceUser.Guid != PredefinedGuids.ServiceUser)
                {
                    return Forbid("You are not allowed to change rulings.");
                }

                if (string.IsNullOrWhiteSpace(ruling.RuleText))
                {
                    return BadRequest("The rule text is required.");
                }
           
                var rulingModel = await repository.Context.Rulings
                    .Include(x => x.Card)
                    .Include(x => x.Creator)
                    .Include(x => x.LastModifiedBy)
                    .Where(x => x.Guid == id).FirstOrDefaultAsync();
                if (rulingModel == null)
                {
                    return NotFound();
                }

                CardModel cardModel = null;
                if (ruling.Card != null)
                {
                    cardModel = await repository.Context.Cards.FindByGuidAsync(ruling.Card.Guid);
                }

                rulingModel.Patch(ruling, cardModel, repository.ServiceUser);
                ruling = rulingModel.FromDal();
             
                await repository.Context.SaveChangesAsync();

                return Ok(ruling);
            }
        }

    }
}
