using System;
using System.Collections.Generic;

namespace PapayaModdingTool.Assets.Script.Wrapper.Json
{
    [Serializable]
    public class LocalizationFile
    {
        public string language;
        public Dictionary<string, string> translations;
    }
}