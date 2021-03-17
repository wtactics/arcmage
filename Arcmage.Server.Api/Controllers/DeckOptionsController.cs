using System;
using System.Collections.Generic;
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
    [Route(Routes.DeckOptions)]
    public class DeckOptionsController : ControllerBase
    {
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {

                var deckModel = await repository.Context.Decks.FindByGuidAsync(id);
                await repository.Context.Entry(deckModel).Reference(x => x.Status).LoadAsync();
                await repository.Context.Entry(deckModel).Reference(x => x.Creator).LoadAsync();

                var deckOptions = new DeckOptions();
                var isMyDeck = deckModel.Creator.Guid == repository.ServiceUser?.Guid;

                deckOptions.IsEditable =
                    (isMyDeck && AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditDeck)) ||
                    (!isMyDeck && AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.AllowOthersDeckEdit));

                deckOptions.IsStatusChangedAllowed =
                    (isMyDeck && AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditDeck)) ||
                    (!isMyDeck && AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.AllowDeckStatusChange));
             
                deckOptions.Statuses = repository.Context.Statuses.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                
                return Ok(deckOptions);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {

                var deckOptions = new DeckOptions();
                deckOptions.Statuses = repository.Context.Statuses.AsNoTracking().ToList().Select(x => x.FromDal()).ToList();
                return Ok(deckOptions);
            }
        }
    }
}
