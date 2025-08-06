using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PapayaModdingTool.Assets.Script.Misc.Localization
{
    public class LanguageUtil
    {
        private static readonly Dictionary<string, Language> StrToLanguageTable = new()
        {
            { "Simplified Chinese", Language.zh },
            { "Traditional Chinese", Language.zh_hant },
            { "English", Language.en },
            { "Spanish", Language.es },
            { "Japanese", Language.ja }
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

        public static string GetDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }
    }
}