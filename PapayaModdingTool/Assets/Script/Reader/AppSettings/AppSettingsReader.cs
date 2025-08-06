using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Localization;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

/*
{
    settings: {
        language: "zh"
    }
}
*/

namespace PapayaModdingTool.Assets.Script.Reader.AppSettings
{
    public class AppSettingsReader
    {
        private readonly IJsonSerializer _jsonSerializer;

        public AppSettingsReader(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public Language ReadLanguage()
        {
            IJsonObject settings = ReadExternalSettings();
            if (settings == null)
                return Language.en; // Fall back to en
            
            string language = settings.GetObject("settings").GetString("language");
            return LanguageUtil.StrToLanguage(language);
        }

        private IJsonObject ReadExternalSettings()
        {
            string settingsFile = PredefinedPaths.AppSettingsFile;
            if (!File.Exists(settingsFile))
                return null;

            string content = File.ReadAllText(settingsFile);
            if (string.IsNullOrWhiteSpace(content.Trim()))
                return null;

            IJsonObject deserialized = _jsonSerializer.DeserializeToObject(content);
            return deserialized;
        }
    }
}