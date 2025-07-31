using System.Collections.Generic;
using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEngine;

public class EditorLocalization
{
    public string CurrentLanguage = "zh";
    private readonly Dictionary<string, IJsonObject> _localizedTexts = new();

    public EditorLocalization(IJsonSerializer jsonSerializer)
    {
        _localizedTexts["en"] = LoadTranslations(Path.Combine(PredefinedPaths.LocalizationPath, "en.json"), jsonSerializer);
        _localizedTexts["zh"] = LoadTranslations(Path.Combine(PredefinedPaths.LocalizationPath, "zh.json"), jsonSerializer);
    }

    public IJsonObject LoadTranslations(string path, IJsonSerializer jsonSerializer)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
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
