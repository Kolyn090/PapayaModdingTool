using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Localization;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEngine;

public class EditorLocalization
{
    public string CurrentLanguage = LanguageUtil.LanguageToStr(Language.en);
    private readonly Dictionary<string, IJsonObject> _localizedTexts = new();

    public EditorLocalization(IJsonSerializer jsonSerializer)
    {
        _localizedTexts[LanguageUtil.LanguageToStr(Language.en)] = LoadTranslations(Path.Combine(PredefinedPaths.LocalizationPath, "en.json"), jsonSerializer);
        _localizedTexts[LanguageUtil.LanguageToStr(Language.zh)] = LoadTranslations(Path.Combine(PredefinedPaths.LocalizationPath, "zh.json"), jsonSerializer);
    }

    public IJsonObject LoadTranslations(string path, IJsonSerializer jsonSerializer)
    {
        if (!File.Exists(path))
        {
            Debug.LogError(string.Format(ELT("file_not_found:")));
            return null;
        }

        string json = File.ReadAllText(path);
        return jsonSerializer.DeserializeToObject(json).GetObject("translations");
    }

    public string ELT(string key)
    {
        if (_localizedTexts.TryGetValue(CurrentLanguage, out var table))
        {
            return table.GetString(key);
        }

        return key; // fallback
    }
}
