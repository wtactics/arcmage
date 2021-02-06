using System;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Game.Api.Assembler;
using Arcmage.Game.Api.GameRuntime;
using Arcmage.Game.Api.Utils;
using Arcmage.Model;
using Microsoft.AspNetCore.Mvc;

namespace Arcmage.Game.Api.Controllers
{
    [Route(Routes.GameSearchOptions)]
    public class GameSearchController : ControllerBase
    {
        public IGameRepository GameRepository { get; }


        public GameSearchController(IGameRepository gameRepository)
        {
            GameRepository = gameRepository;
        }


        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody]  GameSearchOptions searchOptionsBase)
        {
            var dbResult = GameRepository.GetGames().AsQueryable();
              
            if (!string.IsNullOrWhiteSpace(searchOptionsBase.Search))
            {
                dbResult = dbResult.Where(it => it.Name.Contains(searchOptionsBase.Search) );
            }
            var totalCount = dbResult.Count();

            // default order by
            if (string.IsNullOrWhiteSpace(searchOptionsBase.OrderBy))
            {
                searchOptionsBase.OrderBy = "Name";
            }

            var orderByType = QueryHelper.GetPropertyType<Model.Game>(searchOptionsBase.OrderBy);
            if (orderByType != null)
            {
                if (orderByType == typeof(string))
                {
                    var orderByExpression = QueryHelper.GetPropertyExpression<GameRuntime.Game, string>(searchOptionsBase.OrderBy);
                    dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                }
                if (orderByType == typeof(int))
                {
                    var orderByExpression = QueryHelper.GetPropertyExpression<GameRuntime.Game, int>(searchOptionsBase.OrderBy);
                    dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                }
                if (orderByType == typeof(DateTime))
                {
                    var orderByExpression = QueryHelper.GetPropertyExpression<GameRuntime.Game, DateTime>(searchOptionsBase.OrderBy);
                    dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                }
            }

            searchOptionsBase.PageSize = Math.Min(50, searchOptionsBase.PageSize);
            var query = dbResult.Skip((searchOptionsBase.PageNumber - 1) * searchOptionsBase.PageSize).Take(searchOptionsBase.PageSize).Select(x=>x.FromDal());
            var result = new ResultList<Model.Game>(query.ToList()) { TotalItems = totalCount, SearchOptions = searchOptionsBase };
            return Ok(result);
        }
    }
}
