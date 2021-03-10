using System;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Model;
using Arcmage.Model;
using Arcmage.Server.Api.Assembler;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.UserSearchOptions)]
    public class UserSearchController : ControllerBase
    {
        [Authorize]
        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] UserSearchOptions userSearchOptions)
        {
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }
                // only admins can see the users
                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator &&
                    repository.ServiceUser.Role.Guid != PredefinedGuids.ServiceUser)
                {
                    return Forbid();
                }

                var query = repository.Context.Users.Include(x=>x.Role).AsNoTracking();

                if (!string.IsNullOrWhiteSpace(userSearchOptions.Search))
                {
                    query = query.Where(x => x.Name.Contains(userSearchOptions.Search) || x.Email.Contains(userSearchOptions.Search));
                }

                if (userSearchOptions.IsVerified.HasValue)
                {
                    var isVerified = userSearchOptions.IsVerified.Value;
                    query = query.Where(x => x.IsVerified == isVerified);
                }

                if (userSearchOptions.IsDisabled.HasValue)
                {
                    var isDisabled = userSearchOptions.IsDisabled.Value;
                    query = query.Where(x => x.IsDisabled == isDisabled);
                }

                if (userSearchOptions.Role != null)
                {
                    query = query.Where(x => x.Role.Guid == userSearchOptions.Role.Guid);
                }
             
                var totalCount = query.Count();

                // default order by
                if (string.IsNullOrWhiteSpace(userSearchOptions.OrderBy))
                {
                    userSearchOptions.OrderBy = "Name";
                }

                var orderByType = QueryHelper.GetPropertyType<UserModel>(userSearchOptions.OrderBy);
                if (orderByType != null)
                {
                    if (orderByType == typeof(string))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<UserModel, string>(userSearchOptions.OrderBy);
                        query = userSearchOptions.ReverseOrder ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(int))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<UserModel, int>(userSearchOptions.OrderBy);
                        query = userSearchOptions.ReverseOrder ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(DateTime))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<UserModel, DateTime>(userSearchOptions.OrderBy);
                        query = userSearchOptions.ReverseOrder ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression);
                    }
                    if (orderByType == typeof(bool))
                    {
                        var orderByExpression = QueryHelper.GetPropertyExpression<UserModel, bool>(userSearchOptions.OrderBy);
                        query = userSearchOptions.ReverseOrder ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression);
                    }
                }
            
                userSearchOptions.PageSize = Math.Min(50, userSearchOptions.PageSize);

                var userModels = await query.Skip((userSearchOptions.PageNumber - 1)*userSearchOptions.PageSize)
                            .Take(userSearchOptions.PageSize)
                            .ToListAsync();

                var result = new ResultList<User>(userModels.Select(x => x.FromDal(true)).ToList())
                {
                    TotalItems = totalCount,
                    SearchOptions = userSearchOptions
                };
                return Ok(result);
            }
        }
    }
}
