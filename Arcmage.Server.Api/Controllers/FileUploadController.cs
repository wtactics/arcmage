using System;

using System.Threading.Tasks;
using Arcmage.DAL;
using Arcmage.DAL.Utils;
using Arcmage.Model;
using Arcmage.Server.Api.Auth;
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
