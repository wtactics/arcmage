using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Arcmage.Model;

namespace Arcmage.Server.Api.Utils
{
    public static class Languages
    {
        public static List<Language> All { get; set; }

        static Languages()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            All = cultures.Where(x => x.TwoLetterISOLanguageName.Length == 2).Select(x => new Language()
                { Name = x.EnglishName, LanguageCode = x.TwoLetterISOLanguageName }).ToList();
        }

        public static Language GetLanguage(string code)
        {
            var language = All.FirstOrDefault(x => x.LanguageCode == code);
            return language ?? All.FirstOrDefault(x => x.LanguageCode == "en");
        }
    }
}
