using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Auth;
using Arcmage.Server.Api.Layout;
using Arcmage.Server.Api.Utils;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Decks)]
    public class DecksController : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository())
            {
               
                var decks = repository.Context.Decks.Include(x=>x.Status).AsNoTracking().ToList().Select(x => x.FromDal()).ToList();

                var result = new ResultList<Deck>()
                {
                    TotalItems = decks.Count(),
                    Items = decks,

                };
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                await repository.Context.Factions.LoadAsync();
                await repository.Context.CardTypes.LoadAsync();

                var result = await repository.Context.Decks
                    .Include(x=>x.Creator)
                    .Include(x=>x.Status)
                    .Include(x=>x.DeckCards)
                        .ThenInclude(x=>x.Card)
                    .Where(x=>x.Guid == id).FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound();
                }

                var deck = result.FromDal(true);

                await repository.Context.Entry(result).Reference(x => x.Creator).LoadAsync();

                var isMyDeck = deck.Creator.Guid == repository.ServiceUser?.Guid;

                deck.IsEditable =
                    (isMyDeck && AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditDeck)) ||
                    (!isMyDeck && AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.AllowOthersDeckEdit));

                return Ok(deck);
            }
        }

        [HttpGet]
        [Route("{id}/export")]
        [AllowAnonymous]
        public async Task<IActionResult> Export(Guid id, ExportFormat format,  int? tile = 1)
        {
            Repository.InitPaths();
            var deckFile = "";
            var mediaType = "application/zip";
            var extension = ".zip";
            switch (format)
            {
                case ExportFormat.Zip:
                    mediaType = "application/zip";
                    extension = ".zip";
                    deckFile = Repository.GetDeckZipFile(id);
                    if (!System.IO.File.Exists(deckFile)) return NotFound("The deck with the specified id does not exist");
                    break;
                case ExportFormat.Tiles:
                    if (!tile.HasValue) return NotFound("No tile specified");
                    mediaType = "image/png";
                    extension = $"_tile{tile}.png";

                    deckFile = Repository.GetDeckTilesFile(id,tile.Value);
                    if (!System.IO.File.Exists(deckFile)) return NotFound("The deck tile with the specified id and tile does not exist");
                    break;
            }

            using (var repository = new Repository())
            {
                var result = await repository.Context.Decks.Where(x => x.Guid == id).FirstOrDefaultAsync();


                Stream stream = new FileStream(deckFile, FileMode.Open);

                var fileName = result?.Name?.SanitizeFileName();
                if (fileName == null)
                {
                    fileName = $"deck_{id}";
                }
                fileName += extension;
          
                var contentType = new MediaTypeHeaderValue(mediaType);
                var response = new FileStreamResult(stream, contentType) { FileDownloadName = fileName };
                return response;

            }
        }

        /// <summary>
        /// Get all the decks for a user
        /// /api/Decks?userId=xx
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            using (var repository = new Repository())
            {
                var query = await repository.Context.Decks.Where(x=>x.Creator.Guid == userId).ToListAsync();
                var result = new ResultList<Deck>(query.Select(x => x.FromDal()).ToList());
                return Ok(result);
            }
        }



        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] Deck deck)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.CreateDeck))
                {
                    return Forbid();
                }

                if (string.IsNullOrWhiteSpace(deck.Name))
                {
                    return BadRequest("The name is required.");
                }
                var deckModel = repository.CreateDeck(deck.Name, Guid.NewGuid());
                var statusModel = await repository.Context.Statuses.FindByGuidAsync(deck.Status?.Guid);
                deckModel.Patch(deck, statusModel, repository.ServiceUser);

                await repository.Context.SaveChangesAsync();

                return Ok(deckModel.FromDal());
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return BadRequest("Not Implemented");
        }

        [Authorize]
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Deck deck)
        {

            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditDeck))
                {
                    return Forbid();
                }

                await repository.Context.Factions.LoadAsync();
                await repository.Context.CardTypes.LoadAsync();
                var deckModel = await repository.Context.Decks.Include(x => x.DeckCards)
                    .ThenInclude(dc => dc.Card)
                    .Where(x => x.Guid == id).FirstOrDefaultAsync();
                if (deckModel == null)
                {
                    return NotFound();
                }
                await repository.Context.Entry(deckModel).Reference(x => x.Creator).LoadAsync();
                await repository.Context.Entry(deckModel).Reference(x => x.Status).LoadAsync();


                var isMyDeck = deckModel.Creator.Guid == repository.ServiceUser?.Guid;
                if (!isMyDeck && !AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.AllowOthersDeckEdit))
                {
                    return Forbid("This is not your deck");
                }

                StatusModel statusModel = null;
                if (deck.Status != null) {
                    statusModel = await repository.Context.Statuses.FindByGuidAsync(deck.Status.Guid);
                }
                deckModel.Patch(deck, statusModel, repository.ServiceUser);
                deck = deckModel.FromDal(true);
                System.IO.File.WriteAllText(Repository.GetDeckJsonFile(deck.Guid), JsonConvert.SerializeObject(deck));
                System.IO.File.WriteAllText(Repository.GetDeckFormatFile(deck.Guid), GetDeckFormat(deckModel));

                
                var generateMissing = repository.ServiceUser?.Guid == PredefinedGuids.Administrator ||
                                      repository.ServiceUser?.Guid == PredefinedGuids.ServiceUser;

                await repository.Context.SaveChangesAsync();

                BackgroundJob.Schedule(
                    () => DeckGenerator.GenerateDeck(deck.Guid, deck.GeneratePdf, deck.ExportTiles, generateMissing,true), 
                    TimeSpan.FromSeconds(15));

                deck.IsEditable = true;

                return Ok(deck);
            }
        }

        private string GetDeckFormat(DeckModel deckModel)
        {
            string deck = $"# {deckModel.Name} by {deckModel.Creator.Name}" + Environment.NewLine + Environment.NewLine; 
           
            foreach (var deckCardModel in deckModel.DeckCards)
            {
                deck += $"{deckCardModel.Quantity}x {deckCardModel.Card.Name}" + Environment.NewLine;
            }
            return deck;
        }

    }
}
