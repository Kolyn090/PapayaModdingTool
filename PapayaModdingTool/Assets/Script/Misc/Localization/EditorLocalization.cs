using System.Collections.Generic;

public class EL // EditorLocalization
{
    public static string CurrentLanguage = "en";

    private static readonly Dictionary<string, Dictionary<string, string>> localizedTexts =
    new Dictionary<string, Dictionary<string, string>>
    {
        ["en"] = new Dictionary<string, string>
        {
            ["recent_projects"] = "Recent Projects",
            ["open_project"] = "Open",
            ["create_new_project"] = "Create New Project",
            ["select_recent_project"] = "Select a Recent Project",
        },
        ["zh"] = new Dictionary<string, string>
        {
            ["recent_projects"] = "最近项目",
            ["open_project"] = "打开",
            ["create_new_project"] = "创建新项目",
            ["select_recent_project"] = "选择最近项目",
        }
    };

    public static string T(string key)
    {
        if (localizedTexts.TryGetValue(CurrentLanguage, out var table) &&
            table.TryGetValue(key, out var value))
        {
            return value;
        }

        return key; // fallback
    }
}
