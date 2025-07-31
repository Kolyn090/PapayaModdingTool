#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEngine;

public static class EditorLocalization
{
    public static string CurrentLanguage = "zh";

    private static readonly Dictionary<string, Dictionary<string, string>> localizedTexts =
    new()
    {
        ["en"] = LoadTranslations(Path.Combine(PredefinedPaths.LocalizationPath, "en.json")),
        ["zh"] = LoadTranslations(Path.Combine(PredefinedPaths.LocalizationPath, "zh.json"))
    };

    public static Dictionary<string, string> LoadTranslations(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return new Dictionary<string, string>();
        }

        string json = File.ReadAllText(path);
        var localization = JsonConvert.DeserializeObject<LocalizationFile>(json);
        return localization.translations ?? new Dictionary<string, string>();
    }

    public static string ELT(string key)
    {
        if (localizedTexts.TryGetValue(CurrentLanguage, out var table) &&
            table.TryGetValue(key, out var value))
        {
            return value;
        }

        return key; // fallback
    }
}
#endif
