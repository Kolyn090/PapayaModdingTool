using System.Collections.Generic;
using System.Linq;

namespace PapayaModdingTool.Assets.Script.Misc.Localization
{
    public class LanguageUtil
    {
        private static readonly Dictionary<string, Language> StrToLanguageTable = new()
        {
            { "Simplified Chinese", Language.zh },
            { "English", Language.en },
            { "Spanish", Language.es }
        };

        private static Dictionary<Language, string> _languageToStrTable;
        private static Dictionary<Language, string> LanguageToStrTable
        {
            get
            {
                _languageToStrTable ??= StrToLanguageTable.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
                return _languageToStrTable;
            }
        }

        public static string LanguageToStr(Language language)
        {
            if (LanguageToStrTable.ContainsKey(language))
            {
                return LanguageToStrTable[language];
            }
            else
            {
                // Fall back to English
                return LanguageToStrTable[Language.en];
            }
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