using System.Diagnostics;
using System.IO;
using PapayaModdingTool.Assets.Script.Misc.Localization;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.Writer.AppSettings
{
    public class AppSettingsWriter
    {
        private readonly IJsonSerializer _jsonSerializer;

        public AppSettingsWriter(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public bool SetLanguage(Language newLanguage)
        {
            string languageStr = LanguageUtil.LanguageToStr(newLanguage);
            return WriteToSettings("language", languageStr);
        }

        // * All values are string in settings
        // * Will be casted in reader
        private bool WriteToSettings(string field, string value)
        {
            IJsonObject settings = ReadExternalSettings();
            IJsonObject settingsField = settings.GetObject("settings");
            bool writeSuccess = settingsField.SetString(field, value);
            if (!writeSuccess) return false;
            settings.SetObject("settings", settingsField);
            string serialized = _jsonSerializer.SerializeNoFirstLayer(settings);
            File.WriteAllText(PredefinedPaths.AppSettingsFile, serialized);
            return true;
        }

        private IJsonObject ReadExternalSettings()
        {
            string settingsFile = PredefinedPaths.AppSettingsFile;
            if (!Directory.Exists(PredefinedPaths.ExternalDir))
            {
                Directory.CreateDirectory(PredefinedPaths.ExternalDir);
            }
            if (!File.Exists(settingsFile))
            {
                using (File.Create(settingsFile)) { }
            }

            string content = File.ReadAllText(settingsFile).Trim();
            if (string.IsNullOrWhiteSpace(content))
                content = MakeNewSettingsContent();
            IJsonObject deserialized = _jsonSerializer.DeserializeToObject(content);
            return deserialized;
        }

        private string MakeNewSettingsContent()
        {
            return "{ \"settings\": { \"language\": \"English\"}}";
        }
    }
}