using System;
using System.Diagnostics;
using Arcmage.Configuration;

namespace Arcmage.Server.Api.Layout
{
    public static class InkscapeExporter
    {
        private static string InkscapeExe = "\"" + @"C:\Program Files\Inkscape\inkscape.exe" + "\"";
        private static string InkscapePngArgs = "--export-png \"{1}\" --export-area-page --export-dpi {2} --export-width {3} \"{0}\"";

        private static string InkscapePdfArgs = "--export-pdf \"{1}\" --export-area-page --export-dpi {2} \"{0}\"";

        public static void ExportPng(string inputfile, string outputfile, int dpi = 600, int width = 1535)
        {
            try
            {
                // Use inkscape command from setting's or fall back to the default windows location (for backward compatibility)
                var inkscapeExe = Settings.Current.InkscapeExe ?? InkscapeExe;
                var inkscapePngArgs = InkscapePngArgs;

                if (Settings.Current.InkscapeVersion == "1.0.2")
                {
                    inkscapePngArgs = "--export-type=png -o=\"{1}\" --export-area-page --export-dpi={2} --export-width={3} \"{0}\"";
                }

                var args = string.Format(inkscapePngArgs, inputfile, outputfile, dpi, width);

                var processStartInfo = new ProcessStartInfo(inkscapeExe, args);
                processStartInfo.RedirectStandardInput = false;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;

                var process = new Process();
                if (Settings.Current.ForceInkscapeUserImpersonate)
                {
                    ImpersonateUserProcess.Impersonate(process, Settings.Current.InkscapeUser, Settings.Current.InkscapePassword);
                }
               
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
         
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

    

        public static void ExportPdf(string inputfile, string outputfile, int dpi = 600)
        {
            try
            {
                // Use inkscape command from setting's or fall back to the default windows location (for backward compatibility)
                var inkscapeExe = Settings.Current.InkscapeExe ?? InkscapeExe;
                var inkscapePdfArgs = InkscapePdfArgs;

                if (Settings.Current.InkscapeVersion == "1.0.2")
                {
                    inkscapePdfArgs = "--export-type=pdf -o=\"{1}\" --export-area-page --export-dpi={2} \"{0}\"";
                }


                var args = string.Format(inkscapePdfArgs, inputfile, outputfile, dpi);
                var processStartInfo = new ProcessStartInfo(inkscapeExe, args);
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;

                var process = new Process();
                if (Settings.Current.ForceInkscapeUserImpersonate)
                {
                    ImpersonateUserProcess.Impersonate(process, Settings.Current.InkscapeUser, Settings.Current.InkscapePassword);
                }

                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

      

    }
}
