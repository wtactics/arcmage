using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Configuration;
using Arcmage.DAL;
using Arcmage.Model;
using Arcmage.Server.Api.Layout;
using Arcmage.Server.Api.Utils;
using Hangfire.Dashboard.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Arcmage.Server.Api.Controllers
{
    [Route(Routes.Platform)]
    public class PlatformController : ControllerBase
    {

        [HttpGet]
        [Route("StartGameRuntime")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> StartGameRuntime()
        {
       
            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                await repository.Context.Entry(repository.ServiceUser).Reference(x => x.Role).LoadAsync();
                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator ||
                    repository.ServiceUser.Role.Guid != PredefinedGuids.ServiceUser)
                {
                    StartRuntime();
                    return Ok();
                }

                return Forbid();
            }
        }

        [HttpGet]
        [Route("StopGameRuntime")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> StopGameRuntime()
        {

            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                await repository.Context.Entry(repository.ServiceUser).Reference(x => x.Role).LoadAsync();
                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator ||
                    repository.ServiceUser.Role.Guid != PredefinedGuids.ServiceUser)
                {
                    StopRuntime();
                    return Ok();
                }
                return Forbid();
            }
        }

        [HttpGet]
        [Route("RestartGameRuntime")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> RestartGameRuntime()
        {

            using (var repository = new Repository(HttpContext.GetUserGuid()))
            {
                if (repository.ServiceUser == null)
                {
                    return Forbid();
                }

                await repository.Context.Entry(repository.ServiceUser).Reference(x => x.Role).LoadAsync();
                if (repository.ServiceUser.Role.Guid != PredefinedGuids.Administrator ||
                    repository.ServiceUser.Role.Guid != PredefinedGuids.ServiceUser)
                {
                    StopRuntime();
                    StartRuntime();
                    return Ok();
                }
                return Forbid();

            }
        }

        private void StartRuntime()
        {
            var process = GetArcmageGameRuntimeProcess();
            if (process == null)
            {

                var processStartInfo = new ProcessStartInfo("powershell.exe", "-File " + "startgameruntime.ps1");
                processStartInfo.RedirectStandardInput = false;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = false;
                processStartInfo.WorkingDirectory = Settings.Current.GameRuntimePath;

                process = new Process();
                ImpersonateUserProcess.Impersonate(process, Settings.Current.GameRuntimeUser, Settings.Current.GameRuntimeUserPassword);

                process.StartInfo = processStartInfo;
                process.Start();
               
            }
        }

      
        private static Process GetArcmageGameRuntimeProcess()
        {
            return Process.GetProcessesByName("Arcmage.Game.Api").FirstOrDefault();
        }

        private void StopRuntime()
        {
            var process = GetArcmageGameRuntimeProcess();
            if (process != null)
            {
                var processStartInfo = new ProcessStartInfo("powershell.exe", "-File " + "stopgameruntime.ps1");
                processStartInfo.RedirectStandardInput = false;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = false;
                processStartInfo.WorkingDirectory = Settings.Current.GameRuntimePath;

                var killProcess = new Process();
                ImpersonateUserProcess.Impersonate(killProcess, Settings.Current.GameRuntimeUser, Settings.Current.GameRuntimeUserPassword);

                killProcess.StartInfo = processStartInfo;
                killProcess.Start();
                killProcess.WaitForExit();
            }
        }

    }
}
