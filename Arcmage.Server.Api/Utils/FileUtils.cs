using System.IO;
using System.Text.RegularExpressions;

namespace Arcmage.Server.Api.Utils
{
    public static class FileUtils
    {
        public static string SanitizeFileName(this string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            string invalidChars = new string(Path.GetInvalidFileNameChars());
            string escapedInvalidChars = Regex.Escape(invalidChars);
            string invalidRegex = string.Format(@"([{0}]*\.+$)|([{0}]+)", escapedInvalidChars);

            return Regex.Replace(filename, invalidRegex, "_");
        }
    }
}
