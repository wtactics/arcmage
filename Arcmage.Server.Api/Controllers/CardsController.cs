using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.DAL.Utils;
using Arcmage.Layout.InputConvertor;
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


namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Cards)]
    public class CardsController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string search = null)
        {
            using (var repository = new Repository())
            {

                var cardModels = repository.Context.Cards
                    .Include(x => x.RuleSet)
                    .Include(x=>x.Serie)
                    .Include(x=>x.Faction)
                    .Include(x=>x.Status)
                    .Include(x=>x.Type)
                    .Include(x => x.ArtworkLicense)
                    .Include(x => x.Creator)
                    .Include(x => x.LastModifiedBy)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    cardModels = cardModels.Where(it => it.Name.Contains(search) || it.Creator.Name.Contains(search));
                }

                var query = await cardModels.OrderByDescending(it => it.LastModifiedTime).Take(100).ToListAsync();
                var result = new ResultList<Card>(query.Select(x => x.FromDal()).ToList());
                return Ok(result);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Get(Guid id)
        {
            using (var repository = new Repository())
            {
                var result = await repository.Context.Cards.FindByGuidAsync(id);
                if (result == null)
                {
                    return NotFound();
                }
                await repository.Context.Entry(result).Reference(x=>x.Faction).LoadAsync();
                await repository.Context.Entry(result).Reference(x => x.RuleSet).LoadAsync();
                await repository.Context.Entry(result).Reference(x => x.Serie).LoadAsync();
                await repository.Context.Entry(result).Reference(x => x.Type).LoadAsync();
                await repository.Context.Entry(result).Reference(x => x.Status).LoadAsync();
                await repository.Context.Entry(result).Reference(x => x.ArtworkLicense).LoadAsync();
                await repository.Context.Entry(result).Reference(x => x.Creator).LoadAsync();
                await repository.Context.Entry(result).Reference(x => x.LastModifiedBy).LoadAsync();

                return Ok(result.FromDal());
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{id}/rulings")]
        [Produces("application/json")]
        public async Task<IActionResult> GetRulings(Guid id)
        {
            using (var repository = new Repository())
            {
                var rulingModels = await repository.Context.Rulings
                    .Include(x=>x.Creator)
                    .Include(x => x.LastModifiedBy)
                    .Where(x =>x.Card.Guid == id).ToListAsync();
                var result = new ResultList<Ruling>(rulingModels.Select(x => x.FromDal()).OrderByDescending(x=>x.CreateTime).ToList());
                return Ok(result);

            }
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("{id}/export")]
        public async Task<IActionResult> Export(Guid id, ExportFormat format)
        {

            Repository.InitPaths();
            var cardPath = "";
            var mediaType = "";
            
            switch (format)
            {
                case ExportFormat.Art:
                    mediaType = "image/png";
                    cardPath = Repository.GetArtFile(id);
                    break;

                case ExportFormat.Svg:
                    cardPath = Repository.GetSvgFile(id);
                    mediaType = "image/svg+xml";
                    break;

                case ExportFormat.Png:
                    using (var repository = new Repository())
                    {
                        var result = await repository.Context.Cards.FindByGuidAsync(id);
                        if (!string.IsNullOrEmpty(result.PngCreationJobId))
                        {
                            // still busy creating png
                            return NotFound();
                        }
                    }
                    mediaType = "image/png";
                    cardPath = Repository.GetPngFile(id);
                    break;
                case ExportFormat.Jpeg:
                    using (var repository = new Repository())
                    {
                        var result = await repository.Context.Cards.FindByGuidAsync(id);
                        if (!string.IsNullOrEmpty(result.PngCreationJobId))
                        {
                            // still busy creating png
                            return NotFound();
                        }
                    }
                    mediaType = "image/jpeg";
                    cardPath = Repository.GetJpegFile(id);

                    break;
                case ExportFormat.Webp:
                    using (var repository = new Repository())
                    {
                        var result = await repository.Context.Cards.FindByGuidAsync(id);
                        if (!string.IsNullOrEmpty(result.PngCreationJobId))
                        {
                            // still busy creating png
                            return NotFound();
                        }
                    }
                    mediaType = "image/webp";
                    cardPath = Repository.GetWebpFile(id);

                    break;
                case ExportFormat.Pdf:
                    using (var repository = new Repository())
                    {
                        var result = await repository.Context.Cards.FindByGuidAsync(id);
                        if (!string.IsNullOrEmpty(result.PngCreationJobId))
                        {
                            // still busy creating png
                            return NotFound();
                        }
                    }
                    mediaType = "application/pdf";
                    cardPath = Repository.GetPdfFile(id);

                    break;
                case ExportFormat.OverlaySvg:
                    cardPath = Repository.GetOverlaySvgFile(id);
                    mediaType = "image/svg+xml";
                    break;
           
                case ExportFormat.BackgroundPng:
                    using (var repository = new Repository())
                    {
                        var result = await repository.Context.Cards.FindByGuidAsync(id);
                        await repository.Context.Entry(result).Reference(x => x.Faction).LoadAsync();
                        await repository.Context.Entry(result).Reference(x => x.Type).LoadAsync();
                        mediaType = "image/png";
                        cardPath = Repository.GetBackgroundPngFile(result.Faction.Name, result.Type.Name);
                    }
                    break;
                case ExportFormat.BackPng:
                    mediaType = "image/png";
                    cardPath = Repository.GetBackPngFile();
                    break;
                case ExportFormat.BackJpeg:
                    mediaType = "image/jpeg";
                    cardPath = Repository.GetBackJpegFile();
                    break;
                case ExportFormat.BackWebp:
                    mediaType = "image/webp";
                    cardPath = Repository.GetBackWebpFile();
                    break;
                case ExportFormat.BackPdf:
                    mediaType = "application/pdf";
                    cardPath = Repository.GetBackPdfFile();
                    break;
                case ExportFormat.BackSvg:
                    cardPath = Repository.GetBackSvgFile();
                    mediaType = "image/svg+xml";
                    break;
               
                
            }

            if (!System.IO.File.Exists(cardPath)) return NotFound( "The card with the specified id does not exist");

            var filename = Path.GetFileName(cardPath);

            Stream stream = new FileStream(cardPath, FileMode.Open);

            var contentType = new MediaTypeHeaderValue(mediaType);
            var response = new FileStreamResult(stream, contentType) { FileDownloadName = filename };
            return response;
        }


        /// <summary>
        /// Get all the cards for a user
        /// /api/Cards?userId=xx
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            using (var repository = new Repository())
            {
                var query = await repository.Context.Cards.Where(x => x.Creator.Guid == userId).ToListAsync();
                var result = new ResultList<Card>(query.Select(x => x.FromDal()).ToList());
                return Ok(result);
            }
        }

        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] Card card)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.CreateCard))
                {
                    return Forbid();
                }
                
                var cardModel = repository.CreateCard(card.Name, Guid.NewGuid());

                var serieModel = await repository.Context.Series.FindByGuidAsync(card.Serie?.Guid);
                var factionModel = await repository.Context.Factions.FindByGuidAsync(card.Faction?.Guid);
                var cardTypeModel = await repository.Context.CardTypes.FindByGuidAsync(card.Type?.Guid);
                var statusModel = await repository.Context.Statuses.FindByGuidAsync(card.Status?.Guid);
                var ruleSetModel = await repository.Context.RuleSets.FindByGuidAsync(card.RuleSet?.Guid);
                var artworkLicenseModel = await repository.Context.Licenses.FindByGuidAsync(card.ArtworkLicense?.Guid);

                cardModel.Patch(card, serieModel, factionModel, cardTypeModel, statusModel, ruleSetModel, artworkLicenseModel, repository.ServiceUser);

                await repository.Context.SaveChangesAsync();

                await repository.Context.Entry(cardTypeModel).Reference(x => x.TemplateInfo).LoadAsync();
                card = cardModel.FromDal();
                card.Type = cardModel.Type.FromDal(true);

                var cardGenerator = new CardGenerator(card);
                await cardGenerator.Generate();
                await cardGenerator.Generate(false);

                cardModel.PngCreationJobId = BackgroundJob.Schedule(() => CardGenerator.CreatePngJob(card.Guid, card.Faction.Name, card.Type.Name), TimeSpan.FromMinutes(1));
              
                await repository.Context.SaveChangesAsync();

                return Ok(card);
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Card card, bool? forceGeneration)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditCard))
                {
                    return Forbid();
                }

                var cardModel = await repository.Context.Cards.FindByGuidAsync(id);

                await repository.Context.Entry(cardModel).Reference(x => x.Status).LoadAsync();
                var isFinal = cardModel.Status.Guid == PredefinedGuids.Final;
                if (isFinal && !AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.AllowCardStatusChange))
                {
                    return Forbid("Card is marked as final");
                }

                await repository.Context.Entry(cardModel).Reference(x => x.Creator).LoadAsync();
                var isMyCard = repository.ServiceUser?.Guid == cardModel.Creator.Guid;
                if (!isMyCard && !AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.AllowOthersCardEdit))
                {
                    return Forbid("This is not your card");
                }

                await repository.Context.Entry(cardModel).Reference(x => x.Faction).LoadAsync();
                await repository.Context.Entry(cardModel).Reference(x => x.Serie).LoadAsync();
                await repository.Context.Entry(cardModel).Reference(x => x.Type).LoadAsync();
                await repository.Context.Entry(cardModel).Reference(x => x.Status).LoadAsync();
                await repository.Context.Entry(cardModel).Reference(x => x.ArtworkLicense).LoadAsync();

                var serieModel = await repository.Context.Series.FindByGuidAsync(card.Serie.Guid);
                var factionModel = await repository.Context.Factions.FindByGuidAsync(card.Faction.Guid);
                var cardTypeModel = await repository.Context.CardTypes.FindByGuidAsync(card.Type.Guid);
                var statusModel = await repository.Context.Statuses.FindByGuidAsync(card.Status.Guid);
                var ruleSetModel = await repository.Context.RuleSets.FindByGuidAsync(card.RuleSet.Guid);
                var artworkLicenseModel = await repository.Context.Licenses.FindByGuidAsync(card.ArtworkLicense?.Guid);

                bool hasLayoutChanges = forceGeneration.HasValue && forceGeneration.Value;

                if (card.Name != cardModel.Name) hasLayoutChanges = true;
                if (card.Faction.Guid != cardModel.Faction.Guid) hasLayoutChanges = true;
                if (card.Type.Guid != cardModel.Type.Guid) hasLayoutChanges = true;

                if (card.FirstName != cardModel.FirstName) hasLayoutChanges = true;
                if (card.LastName != cardModel.LastName) hasLayoutChanges = true;
                if (card.Artist != cardModel.Artist) hasLayoutChanges = true;
                if (card.SubType != cardModel.SubType) hasLayoutChanges = true;
                if (card.Cost != cardModel.Cost) hasLayoutChanges = true;
                if (card.Loyalty != cardModel.Loyalty) hasLayoutChanges = true;
                if (card.Attack != cardModel.Attack) hasLayoutChanges = true;
                if (card.Defense != cardModel.Defense) hasLayoutChanges = true;
                if (card.Info != cardModel.Info) hasLayoutChanges = true;
               
                if (card.MarkdownText != cardModel.MarkdownText) hasLayoutChanges = true;

               

                cardModel.Patch(card, serieModel, factionModel, cardTypeModel, statusModel, ruleSetModel, artworkLicenseModel, repository.ServiceUser);
                await repository.Context.SaveChangesAsync();

                await repository.Context.Entry(cardTypeModel).Reference(x => x.TemplateInfo).LoadAsync();
                card = cardModel.FromDal();
                card.Type = cardModel.Type.FromDal(true);
                await repository.Context.SaveChangesAsync();

                if (hasLayoutChanges)
                {
                    var jobId = cardModel.PngCreationJobId;
                    if (!string.IsNullOrEmpty(jobId))
                    {
                        BackgroundJob.Delete(jobId);
                    }

                    var cardGenerator = new CardGenerator(card);
                    await cardGenerator.Generate();
                    await cardGenerator.Generate(false);

                    cardModel.PngCreationJobId = BackgroundJob.Schedule(() => CardGenerator.CreatePngJob(card.Guid, card.Faction.Name, card.Type.Name), TimeSpan.FromMinutes(1));
                }
                await repository.Context.SaveChangesAsync();

                return Ok(cardModel.FromDal());
            }
        }

       

    }
}
