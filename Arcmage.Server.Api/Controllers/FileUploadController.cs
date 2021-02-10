using System;

using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arcmage.Server.Api.Controllers
{
  
    [Route(Routes.FileUpload)]
    public class FileUploadController : ControllerBase
    {
        [HttpPost, DisableRequestSizeLimit]
        [Authorize]
        [Route("{id}")]
        public async Task<IActionResult> Upload(Guid id)
        {


            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                var cardModel = await repository.Context.Cards.FindByGuidAsync(id);

                await repository.Context.Entry(cardModel).Reference(x => x.Status).LoadAsync();
                await repository.Context.Entry(cardModel).Reference(x => x.Creator).LoadAsync();

                if (repository.ServiceUser != null)
                {
                    await repository.Context.Entry(repository.ServiceUser).Reference(x => x.Role).LoadAsync();
                    if (cardModel.Status.Guid == PredefinedGuids.Final)
                    {
                        if (repository.ServiceUser.Role.Guid == PredefinedGuids.Developer ||
                            repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator ||
                            repository.ServiceUser.Role.Guid != PredefinedGuids.ServiceUser)
                        {
                            return Forbid("Card is marked as final");
                        }
                    }
                }
            }


            Repository.InitPaths();


            var artFile = Repository.GetArtFile(id);
            if (System.IO.File.Exists(artFile))
            {
                try
                {
                    System.IO.File.Delete(artFile);
                }
                catch (Exception)
                {
                    return Conflict();
                }
            }
            
            using (var fileStream = System.IO.File.Create(artFile))
            {
                try
                {
                    var file = Request.Form.Files[0];
                    await file.CopyToAsync(fileStream);
                    return Ok() ;
                }
                catch (Exception e)
                {
                    return StatusCode(500, $"Internal server error: {e}");
                }
            }
        }
    }
}
