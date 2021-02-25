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
                // N.G. Somehow the msbuild publish doesn't copy the correct version of Microsoft.Data.SqlClient to the game runtime folder
                // Here's a hack that fixes this for windows-64 bit
                CopyGameRuntimeRequirements();

                var processStartInfo = new ProcessStartInfo(Path.Combine(Settings.Current.GameRuntimePath, GameRuntimeName));
                processStartInfo.RedirectStandardInput = false;
                processStartInfo.UseShellExecute = true;
                processStartInfo.CreateNoWindow = false;
                processStartInfo.WorkingDirectory = Settings.Current.GameRuntimePath;

                process = new Process();
                ImpersonateUserProcess.Impersonate(process, Settings.Current.GameRuntimeUser, Settings.Current.GameRuntimeUserPassword);

                process.StartInfo = processStartInfo;
                process.Start();
               
            }
        }

        private static void CopyGameRuntimeRequirements()
        {

            var processStartInfo = new ProcessStartInfo(Path.Combine(Settings.Current.GameRuntimePath, "copygameruntimerequirements.bat"));
            processStartInfo.RedirectStandardInput = false;
            processStartInfo.UseShellExecute = true;
            processStartInfo.CreateNoWindow = false;
            processStartInfo.WorkingDirectory = Settings.Current.GameRuntimePath;

            var process = new Process();
            ImpersonateUserProcess.Impersonate(process, Settings.Current.GameRuntimeUser, Settings.Current.GameRuntimeUserPassword);

            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
        }

        private static string GameRuntimeName = $"Arcmage.Game.Api.exe";

        private static Process GetArcmageGameRuntimeProcess()
        {
            return Process.GetProcessesByName(GameRuntimeName).FirstOrDefault();
        }

        private void StopRuntime()
        {
            var process = GetArcmageGameRuntimeProcess();
            if (process != null)
            {
                ImpersonateUserProcess.Impersonate(process, Settings.Current.GameRuntimeUser, Settings.Current.GameRuntimeUserPassword);
                process.Kill(true);
                process.WaitForExit();
            }
        }

    }
}
