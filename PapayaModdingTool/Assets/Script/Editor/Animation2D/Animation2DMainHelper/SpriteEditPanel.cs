using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.EventListener;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D.Animation2DMainHelper
{
    public class SpriteEditPanel : ISpriteButtonDataListener
    {
        public Func<string, string> ELT;

        private Texture2D _sprite;
        private int _level;
        private int _order;
        private string _animation;
        private string _name;
        private float _width;
        private float _height;
        List<string> options = new() { "Option A", "Option B", "Option C" };
        int selectedIndex = 0; // currently selected index
        private bool _hasInit;

        private Rect _bound;

        float fieldWidth = 140f; // width of the input box
        float labelWidth = 60f;  // width of the label
        float spacing = 5f;      // space between label and field
        
        public void Initialize(Rect bound)
        {
            _bound = bound;
            _hasInit = true;
        }

        public void CreatePanel()
        {
            if (!_hasInit)
                return;

            EditorGUI.DrawRect(_bound, new Color(0.2f, 0.2f, 0.2f));
            GUILayout.BeginArea(_bound);

            EditorGUI.BeginDisabledGroup(_sprite == null);
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    DrawImageDisplay();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    DrawField("Name", ref _name);
                    DrawField("Level", ref _level);
                    DrawField("Order", ref _order);
                    DrawField("Width", ref _width);
                    DrawField("Height", ref _height);
                    // DrawField("Animation", ref _animation);

                    // Draw the popup
                    DrawDropdownList("Animation", ref selectedIndex, options);

                    // Optional: access the selected string
                    // string selectedValue = options[selectedIndex];
                    // EditorGUILayout.LabelField("Selected: " + selectedValue);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.Label("");
                    GUILayout.Label("");
                    GUILayout.Label("");
                    GUILayout.Label("");
                    GUILayout.Label("");
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin

                        if (GUILayout.Button("Play Animation", GUILayout.Width(100)))
                        {

                        }

                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label("");
                    GUILayout.Label("");

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin

                        if (GUILayout.Button("Add To Preview", GUILayout.Width(100)))
                        {
                            
                        }

                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            GUILayout.EndArea();
        }

        private void DrawField<T>(string label, ref T value)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new(rect.x, rect.y, labelWidth, rect.height);
            Rect fieldRect = new(rect.x + labelWidth + spacing, rect.y, fieldWidth, rect.height);
            
            EditorGUI.LabelField(labelRect, label);

            if (typeof(T) == typeof(int))
                value = (T)(object)EditorGUI.IntField(fieldRect, (int)(object)value);
            else if (typeof(T) == typeof(float))
                value = (T)(object)EditorGUI.FloatField(fieldRect, (float)(object)value);
            else if (typeof(T) == typeof(string))
                value = (T)(object)EditorGUI.TextField(fieldRect, (string)(object)value);
        }

        private void DrawDropdownList(string label, ref int value, List<string> options, string nullLabel = "<None>")
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new(rect.x, rect.y, labelWidth, rect.height);
            Rect fieldRect = new(rect.x + labelWidth + spacing, rect.y, fieldWidth, rect.height);

            EditorGUI.LabelField(labelRect, label);

            // Prepare options array with null option at index 0
            string[] optionArray = new string[options.Count + 1];
            optionArray[0] = nullLabel; // represents null
            for (int i = 0; i < options.Count; i++)
                optionArray[i + 1] = options[i];

            // Remember previous value
            int previousValue = value;

            // Draw the popup
            value = EditorGUI.Popup(fieldRect, value, optionArray);

            // value == 0 means null / none selected

            // Invoke callback if selection changed
            if (value != previousValue )
            {
                Debug.Log(value > 0 ? $"Changed to {options[value-1]}" : "Changed to None");
            }
        }

        private void DrawImageDisplay(params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical("box", options);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // left flexible space

            GUILayout.BeginVertical(GUILayout.Width(128));
            GUILayout.FlexibleSpace(); // push image down if vertical space

            if (_sprite != null)
            {
                GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                GUILayout.Label(DoubleSize(_sprite), centeredStyle, GUILayout.ExpandHeight(true));
            }
            else
            {
                GUILayout.Label("", GUILayout.ExpandHeight(true));
            }

            GUILayout.FlexibleSpace(); // push image up if vertical space
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); // right flexible space
            GUILayout.EndHorizontal();

            string label = !string.IsNullOrWhiteSpace(_name) ? _name : "<null>";
            GUILayout.Label(
                TruncateToEnd(label, 25),
                EditorStyles.centeredGreyMiniLabel,
                GUILayout.ExpandWidth(true)
            );

            GUILayout.EndVertical();
        }

        public static Texture2D DoubleSize(Texture2D source, int factor=2)
        {
            int width = source.width * factor;
            int height = source.height * factor;

            Texture2D result = new(width, height, source.format, false);

            result.filterMode = FilterMode.Point; // ensure point sampling
            source.filterMode = FilterMode.Point;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Map to source coordinates using nearest integer
                    int srcX = x / factor;
                    int srcY = y / factor;
                    Color col = source.GetPixel(srcX, srcY);
                    result.SetPixel(x, y, col);
                }
            }

            result.Apply();
            return result;
        }

        private string TruncateToEnd(string s, int len = 20)
        {
            return s.Length > len ? $"...{s[^len..]}" : s;
        }


        public void Update(SpriteButtonData data)
        {
            int animIndex = options.IndexOf(data.animation);
            if (animIndex >= 0)
                selectedIndex = animIndex + 1;

            _sprite = data.sprite;
            _level = data.level;
            _order = data.order;
            _animation = data.animation;
            _name = data.label;
            _width = data.width;
            _height = data.height;
        }
    }
}