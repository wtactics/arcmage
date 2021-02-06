using System.Diagnostics;

namespace Arcmage.Server.Api.Layout
{
    public class ImpersonateUserProcess
    {
        public static void Impersonate(Process process, string username, string password)
        {
            System.Security.SecureString ssPwd = new System.Security.SecureString();
            process.StartInfo.UserName = username;
            for (int x = 0; x < password.Length; x++)
            {
                ssPwd.AppendChar(password[x]);
            }
            process.StartInfo.Password = ssPwd;
        }
    }
}
