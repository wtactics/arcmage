using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.DeckSearchOptions)]
    public class DeckSearchController : ControllerBase
    {
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] DeckSearchOptions deckSearchOptions)
        {
            using (var repository = new Repository())
            {
                var dbResult = repository.Context.Decks
                    .Include(x=>x.Status)
                    .Include(x => x.Creator).AsNoTracking();

                if (!string.IsNullOrWhiteSpace(deckSearchOptions.Search))
                {
                    dbResult =
                        dbResult.Where(
                            it =>
                                it.Name.Contains(deckSearchOptions.Search) ||
                                it.Creator.Name.Contains(deckSearchOptions.Search));
                }

                if (deckSearchOptions.ExportTiles.HasValue)
                {
                    var exportTiles = deckSearchOptions.ExportTiles.Value;
                    dbResult = dbResult.Where(x => x.ExportTiles == exportTiles);
                }

                if (deckSearchOptions.MyDecks.HasValue && deckSearchOptions.MyDecks.Value)
                {
                    var userGuid = HttpContext.GetUserGuid();
                    if (userGuid != Guid.Empty)
                    {
                        dbResult = dbResult.Where(x => x.Creator.Guid == userGuid);
                    }
                }

                if (deckSearchOptions.Status == null && deckSearchOptions.ExcludeDrafts.HasValue && deckSearchOptions.ExcludeDrafts.Value)
                {
                    dbResult = dbResult.Where(x => x.Status == null || x.Status.Guid != PredefinedGuids.Draft);
                }
                

                if (deckSearchOptions.Status != null)
                {
                    dbResult = dbResult.Where(x => x.Status != null && x.Status.Guid == deckSearchOptions.Status.Guid);
                }

                var totalCount = dbResult.Count();

                // default order by
                if (string.IsNullOrWhiteSpace(deckSearchOptions.OrderBy))
                {
                    deckSearchOptions.OrderBy = "Name";
                }

                var orderByType = QueryHelper.GetPropertyType<DeckModel>(deckSearchOptions.OrderBy);
                if (orderByType != null)
                {
                    if (orderByType == typeof(string))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<DeckModel, string>(deckSearchOptions.OrderBy);
                        dbResult = deckSearchOptions.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(int))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<DeckModel, int>(deckSearchOptions.OrderBy);
                        dbResult = deckSearchOptions.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(DateTime))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<DeckModel, DateTime>(deckSearchOptions.OrderBy);
                        dbResult = deckSearchOptions.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                }


                deckSearchOptions.PageSize = Math.Min(50, deckSearchOptions.PageSize);

                var query =
                    await
                        dbResult.Skip((deckSearchOptions.PageNumber - 1)*deckSearchOptions.PageSize)
                            .Take(deckSearchOptions.PageSize)
                            .ToListAsync();

                var result = new ResultList<Deck>(query.Select(x => x.FromDal()).ToList())
                {
                    TotalItems = totalCount,
                    SearchOptions = deckSearchOptions
                };
                return Ok(result);
            }
        }
    }
}
