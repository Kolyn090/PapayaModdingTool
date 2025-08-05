using System;
using PapayaModdingTool.Assets.Script.Misc.Localization;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Universal.ProjectManagerHelper
{
    public class SwitchLanguageHelper
    {
        public Func<string, string> ELT;
        public Action CloseCurrWindow;
        public Action ShowCurrWindow;
        public Func<Language> ReadLanguage;
        public Func<Language, bool> SetLanguage;

        private Language _language;
        private Language _lastLanguage;

        public void Initialize()
        {
            _language = ReadLanguage();
            _lastLanguage = _language;
        }

        public void CreateSwitchLanguagePanel()
        {
            Language[] values = (Language[])Enum.GetValues(typeof(Language));
            string[] displayNames = Array.ConvertAll(values, v => LanguageUtil.GetDescription(v));

            int selectedIndex = Array.IndexOf(values, _language);
            int newIndex = EditorGUILayout.Popup(ELT("switch_language"), selectedIndex, displayNames);

            if (newIndex != selectedIndex)
            {
                _language = values[newIndex];
            }

            if (_lastLanguage != _language)
            {
                _lastLanguage = _language;
                bool setLanguageSuccess = SetLanguage(_language);
                if (setLanguageSuccess)
                {
                    setLanguageSuccess = false;
                    Debug.Log(string.Format(ELT("set_language_success"), LanguageUtil.LanguageToStr(_language)));

                    // ! Close and reopen the window
                    CloseCurrWindow();
                    EditorApplication.delayCall += () => ShowCurrWindow();
                }
            }
        }
    }
}