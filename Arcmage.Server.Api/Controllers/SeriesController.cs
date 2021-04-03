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

        [HttpPost]
        [AllowAnonymous]
        [Produces("application/json")]
        [Route("search")]
        public async Task<IActionResult> Post([FromBody] SearchOptionsBase searchOptionsBase)
        {
            using (var repository = new Repository())
            {

                IQueryable<SerieModel> dbResult = repository.Context.Series
                    .Include(x => x.Creator)
                    .Include(x => x.LastModifiedBy).AsNoTracking();

  

                if (!string.IsNullOrWhiteSpace(searchOptionsBase.Search))
                {
                    dbResult = dbResult.Where(
                        it => it.Name.Contains(searchOptionsBase.Search) 
                    );
                }
                var totalCount = dbResult.Count();

                // default order by
                if (string.IsNullOrWhiteSpace(searchOptionsBase.OrderBy))
                {
                    searchOptionsBase.OrderBy = "Name";
                }

                var orderByType = QueryHelper.GetPropertyType<SerieModel>(searchOptionsBase.OrderBy);
                if (orderByType != null)
                {
                    if (orderByType == typeof(string))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<SerieModel, string>(searchOptionsBase.OrderBy);
                        dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(int))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<SerieModel, int>(searchOptionsBase.OrderBy);
                        dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(DateTime))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<SerieModel, DateTime>(searchOptionsBase.OrderBy);
                        dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                }

                searchOptionsBase.PageSize = Math.Min(50, searchOptionsBase.PageSize);
                var query = await dbResult.Skip((searchOptionsBase.PageNumber - 1) * searchOptionsBase.PageSize).Take(searchOptionsBase.PageSize).ToListAsync();
                var result = new ResultList<Serie>(query.Select(x => x.FromDal()).ToList()) { TotalItems = totalCount, SearchOptions = searchOptionsBase };
                return Ok(result);
            }
        }
    }

}
