using System;
using System.Diagnostics;
using System.Linq;

namespace GameSshUpdater
{
    public class Program
    {

        public static string Netsh = @"C:\Windows\System32\netsh";

        public static string NetshShowHostname = "http show sslcert hostname={0}";
        public static string NetshShowIpport = "http show sslcert ipport={0}";

        public static string DeleteSSLCert = "http del sslcert ipport={0}";
        public static string AddSSLCert = "http add sslcert ipport={0} certhash={1} appid={{{2}}} certstore={3}";

        static void Main(string[] args)
        {
            //var certhash = GetSslHashByHostname("aminduna.arcmage.org:443");
            var certhash = GetSslHashByIpport("0.0.0.0:5000");
            var certstore = "WebHosting";

            if (certhash != null)
            {
                // Aminduna game runtime
                var gameRuntimeAppId = "512ae165-c8ac-4107-b079-e041623c0bdf";
                var gameRuntimeIpPort = "0.0.0.0:5090";
                DeleteSsl(gameRuntimeIpPort);
                AddSsl(gameRuntimeIpPort, certhash, gameRuntimeAppId, certstore);

                //// Aminduna alpha website
                //var amindunaAppId = "407b5d98-c3f2-489f-b69a-9c20eda33880";
                //var amindunaIpPort = "0.0.0.0:5000";
                //DeleteSsl(amindunaIpPort);
                //AddSsl(amindunaIpPort, certhash, amindunaAppId, certstore);
            }
        }


        public static string GetSslHashByHostname(string hostname)
        {
            var command = string.Format(NetshShowHostname, hostname);
            var output = ExecuteNetshCommand(command, true);
            return GetHashFromOutput(output);
        }

        public static string GetSslHashByIpport(string ipport)
        {
            var command = string.Format(NetshShowIpport, ipport);
            var output = ExecuteNetshCommand(command, true);
            return GetHashFromOutput(output);
        }

        private static string GetHashFromOutput(string output)
        {
            var hashKey = output.Split('\n').Select(x => x.TrimEnd('\n', '\r'))
                .FirstOrDefault(x => x.StartsWith("    Certificate Hash             : "));
            var hash = hashKey?.Substring("    Certificate Hash             : ".Length);
            hash = hash?.Trim();
            return hash;
        }

        public static void DeleteSsl(string ipport)
        {
            var command = string.Format(DeleteSSLCert, ipport);
            ExecuteNetshCommand(command);
        }

        public static void AddSsl(string ipport, string certhash, string appid, string certstore)
        {
            var command = string.Format(AddSSLCert, ipport, certhash, appid, certstore);
            ExecuteNetshCommand(command);
        }


        public static string ExecuteNetshCommand(string args, bool readOutput = false)
        {
            string output = null;
            try
            {
                var processStartInfo = new ProcessStartInfo(Netsh, args);
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;

                var process = new Process();
                // Impersonate(process);
                process.StartInfo = processStartInfo;
                process.Start();
                if (readOutput)
                {
                    output = process.StandardOutput.ReadToEnd();
                }
                process.WaitForExit();
            }
            catch (Exception e)
            {
            }
            return output;
        }


     
    }
}
