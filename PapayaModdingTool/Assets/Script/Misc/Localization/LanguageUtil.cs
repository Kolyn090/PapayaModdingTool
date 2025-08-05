using System.Collections.Generic;

namespace PapayaModdingTool.Assets.Script.Misc.Localization
{
    public class LanguageUtil
    {
        private static readonly Dictionary<string, Language> StrToLanguageTable = new()
        {
            { "zh", Language.zh },
            { "en", Language.en }
        };

        public static string LanguageToStr(Language language)
        {
            return language.ToString().ToLower();
        }

        public static Language StrToLanguage(string s)
        {
            if (StrToLanguageTable.ContainsKey(s))
            {
                return StrToLanguageTable[s];
            }
            else
            {
                // Fall back to English
                return Language.en;
            }
        }
    }
}