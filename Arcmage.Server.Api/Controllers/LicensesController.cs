using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.DAL.Utils;
using Arcmage.Game.Api.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Licenses)]
    public class LicensesController : ControllerBase
    {
        [HttpGet]
        [Produces("application/json")]
        public async Task<IActionResult> Get()
        {
            using (var repository = new Repository())
            {
                var query = await repository.Context.Licenses.ToListAsync();
                var result = new ResultList<License>(query.Select(x => x.FromDal()).ToList());
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
                var result = await repository.Context.Licenses.FindByGuidAsync(id);
                return Ok(result.FromDal());
            }
        }

        [HttpPost]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] License license)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.CreateLicense))
                {
                    return Forbid("You are not allowed to create licenses.");
                }
                if (string.IsNullOrWhiteSpace(license.Name))
                {
                    return BadRequest("The name is required.");
                }
                if (string.IsNullOrWhiteSpace(license.Description))
                {
                    return BadRequest("The description is required.");
                }
                if (string.IsNullOrWhiteSpace(license.Url))
                {
                    return BadRequest("The url is required.");
                }
                var licenseModel = repository.CreateLicense(license.Name, license.Description, license.Url, Guid.NewGuid());
                return Ok(licenseModel.FromDal());
            }
        }

        [Authorize]
        [HttpPatch]
        [Produces("application/json")]
        [Route("{id}")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] License license)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.EditLicense))
                {
                    return Forbid("You are not allowed to edit licenses.");
                }
                if (string.IsNullOrWhiteSpace(license.Name))
                {
                    return BadRequest("The name is required.");
                }
                if (string.IsNullOrWhiteSpace(license.Description))
                {
                    return BadRequest("The description is required.");
                }
                if (string.IsNullOrWhiteSpace(license.Url))
                {
                    return BadRequest("The url is required.");
                }
                var licenseModel = await repository.Context.Licenses.FindByGuidAsync(id);
                licenseModel.Patch(license, repository.ServiceUser);
                await repository.Context.SaveChangesAsync();
                return Ok(licenseModel.FromDal());
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

                IQueryable<LicenseModel> dbResult = repository.Context.Licenses
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
                        var orderByExpression = QueryHelper.GetPropertyExpression<LicenseModel, string>(searchOptionsBase.OrderBy);
                        dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(int))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<LicenseModel, int>(searchOptionsBase.OrderBy);
                        dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(DateTime))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<LicenseModel, DateTime>(searchOptionsBase.OrderBy);
                        dbResult = searchOptionsBase.ReverseOrder ? dbResult.OrderByDescending(orderByExpression) : dbResult.OrderBy(orderByExpression);
                    }
                }

                searchOptionsBase.PageSize = Math.Min(50, searchOptionsBase.PageSize);
                var query = await dbResult.Skip((searchOptionsBase.PageNumber - 1) * searchOptionsBase.PageSize).Take(searchOptionsBase.PageSize).ToListAsync();
                var result = new ResultList<License>(query.Select(x => x.FromDal()).ToList()) { TotalItems = totalCount, SearchOptions = searchOptionsBase };
                return Ok(result);
            }
        }
    }

}
