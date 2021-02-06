using System;
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
    [Route(Routes.CardOptions)]
    public class CardOptionsController : ControllerBase
    {
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
               

                var card = await repository.Context.Cards.FindByGuidAsync(id);
                await repository.Context.Entry(card).Reference(x => x.Status).LoadAsync();
                await repository.Context.Entry(card).Reference(x => x.Creator).LoadAsync();

                var cardOptions = new CardOptions();
                if (repository.ServiceUser != null)
                {
                    await repository.Context.Entry(repository.ServiceUser).Reference(x => x.Role).LoadAsync();
                    if (card.Status.Guid != PredefinedGuids.Final)
                    {
                        if (card.Creator.Guid == repository.ServiceUser.Guid) cardOptions.IsEditable = true;
                    }
                    if (repository.ServiceUser.Role.Guid == PredefinedGuids.Developer ||
                        repository.ServiceUser.Role.Guid == PredefinedGuids.Administrator ||
                        repository.ServiceUser.Role.Guid == PredefinedGuids.ServiceUser)
                    {
                        cardOptions.IsEditable = true;
                        cardOptions.IsStatusChangedAllowed = true;
                    }
                }

                cardOptions.Factions = repository.Context.Factions.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                cardOptions.Series = repository.Context.Series.AsNoTracking().ToList().Select(x => x.FromDal(false)).ToList();
                cardOptions.RuleSets = repository.Context.RuleSets.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                cardOptions.Statuses = repository.Context.Statuses.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                cardOptions.CardTypes = repository.Context.CardTypes.Include(x => x.TemplateInfo).AsNoTracking().ToList().Select(x => x.FromDal(true)).ToList();

                return Ok(cardOptions);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
              
                var cardOptions = new CardOptions();
                cardOptions.Factions = repository.Context.Factions.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                cardOptions.Series = repository.Context.Series.AsNoTracking().ToList().Select(x => x.FromDal(false)).ToList();
                cardOptions.RuleSets = repository.Context.RuleSets.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                cardOptions.Statuses = repository.Context.Statuses.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                cardOptions.CardTypes = repository.Context.CardTypes.Include(x => x.TemplateInfo).AsNoTracking().ToList().Select(x => x.FromDal(true)).ToList();

                return Ok(cardOptions);
            }
        }
    }
}
