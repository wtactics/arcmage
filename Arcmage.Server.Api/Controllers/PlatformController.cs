using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Configuration;
using Arcmage.DAL;
using Arcmage.Model;
using Arcmage.Server.Api.Auth;
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
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.GameRuntimeAdmin))
                {
                    return Forbid();
                }
                StartRuntime();
                return Ok();
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
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.GameRuntimeAdmin))
                {
                    return Forbid();
                }
                StopRuntime();
                return Ok();
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
                if (!AuthorizeService.HashRight(repository.ServiceUser?.Role, Rights.GameRuntimeAdmin))
                {
                    return Forbid();
                }
                StopRuntime();
                StartRuntime();
                return Ok();
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
