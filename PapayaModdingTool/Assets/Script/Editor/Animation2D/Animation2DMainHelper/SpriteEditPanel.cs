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
        private const float Field_Width = 140f; // width of the input box
        private const float Label_Width = 60f;  // width of the label
        private const float Spacing = 5f;      // space between label and field

        public Func<string, string> ELT;
        public Func<List<SpriteButtonData>> GetDatas; // Workplace
        public Action<List<SpriteButtonData>> SetDatas; // Workplace

        private Texture2D _sprite;
        private int _level;
        private int _order;
        private string _animation;
        private string _newAnimation;
        private string _deleteAnimation;
        private string _name;
        private int _width;
        private int _height;
        private SpriteButtonData _curr;
        private List<string> _animations = new();
        private int _selectedIndex = 0; // currently selected index
        private bool _hasInit;

        private Rect _bound;
        
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
                    DrawField(ELT("sprite_edit_name"), ref _name, var => { if (_curr != null) _curr.label = var; });
                    DrawField(ELT("sprite_edit_level"), ref _level, var => {  if (_curr != null) _curr.level = var; });
                    DrawField(ELT("sprite_edit_order"), ref _order, var => { if (_curr != null) _curr.order = var; });
                    DrawField(ELT("sprite_edit_width"), ref _width, var => {  if (_curr != null) _curr.width = var; });
                    DrawField(ELT("sprite_edit_height"), ref _height, var => { if (_curr != null) _curr.height = var; });
                    DrawField(ELT("create_new_animation"), ref _newAnimation);
                    DrawField(ELT("delete_animation"), ref _deleteAnimation);
                    DrawDropdownList(ELT("sprite_edit_animation"),
                                        ref _selectedIndex,
                                        _animations,
                                        var => { _animation = var; if (_curr != null) _curr.animation = var; });
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin

                        if (GUILayout.Button(ELT("flip_x"), GUILayout.Width(100)))
                        {
                            FlipX();
                        }

                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin

                        if (GUILayout.Button(ELT("flip_y"), GUILayout.Width(100)))
                        {
                            FlipY();
                        }

                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label("");
                    GUILayout.Label("");
                    GUILayout.Label("");

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin
                        if (GUILayout.Button(ELT("sprite_edit_create"), GUILayout.Width(100)))
                        {
                            AddAnimation();
                        }
                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin
                        if (GUILayout.Button(ELT("sprite_edit_delete"), GUILayout.Width(100)))
                        {
                            DeleteAnimation();
                        }
                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin
                        if (GUILayout.Button(ELT("play_animation"), GUILayout.Width(100)))
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
                        if (GUILayout.Button(ELT("update_workplace"), GUILayout.Width(100), GUILayout.Height(40)))
                        {
                            UpdateWorkplace();
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

        private void AddAnimation()
        {
            if (string.IsNullOrWhiteSpace(_newAnimation))
            {
                Debug.LogWarning("Warning: New animation is empty. Abort creation.");
            }
            else
            {
                if (!_animations.Contains(_newAnimation))
                {
                    _animations.Add(_newAnimation);
                    _newAnimation = "";
                    Debug.Log($"Created new animation: {_newAnimation}");
                }
            }
        }

        private void DeleteAnimation()
        {
            if (string.IsNullOrWhiteSpace(_deleteAnimation))
            {
                Debug.LogWarning("Warning: Animation to delete is empty. Abort deletion.");
            }
            else
            {
                if (!_animations.Contains(_deleteAnimation))
                {
                    Debug.LogWarning($"Doesn't contain animation: {_deleteAnimation}. Abort deletion.");
                }
                else
                {
                    _animations.Remove(_deleteAnimation);
                    _deleteAnimation = "";
                    Debug.Log($"Deleted animation: {_deleteAnimation}.");
                }
            }
        }

        private void UpdateWorkplace()
        {
            if (GetDatas == null || SetDatas == null)
            {
                Debug.LogError("Did you forgot to assign Workplace Datas?");
                return;
            }

            if (_sprite == null)
            {
                Debug.LogWarning("No sprite selected. Abort.");
                return;
            }

            if (GetDatas() == null)
            {
                SetDatas(new() { _curr });
            }
            else
            {
                if (GetDatas().Contains(_curr))
                {
                    _curr.level = _level;
                    _curr.order = _order;
                    _curr.animation = _animation;
                    _curr.label = _name;
                    _curr.width = _width;
                    _curr.height = _height;
                    SetDatas(GetDatas());
                }
                else
                {
                    _curr.level = _level;
                    _curr.order = _order;
                    _curr.animation = _animation;
                    _curr.label = _name;
                    _curr.width = _width;
                    _curr.height = _height;
                    List<SpriteButtonData> copy = new(GetDatas())
                    {
                        _curr
                    };
                    SetDatas(copy);
                }
            }
        }

        private void DrawField<T>(string label,
                                    ref T value,
                                    Action<T> callBack = null)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new(rect.x, rect.y, Label_Width, rect.height);
            Rect fieldRect = new(rect.x + Label_Width + Spacing, rect.y, Field_Width, rect.height);

            EditorGUI.LabelField(labelRect, label);

            Color originalColor = GUI.contentColor;

            // Check if value is numeric and less than 0
            if (typeof(T) == typeof(int) && (int)(object)value < 0)
            {
                GUI.contentColor = Color.red;
                value = (T)(object)EditorGUI.IntField(fieldRect, (int)(object)value);
            }
            else if (typeof(T) == typeof(float) && (float)(object)value < 0f)
            {
                GUI.contentColor = Color.red;
                value = (T)(object)EditorGUI.FloatField(fieldRect, (float)(object)value);
            }
            else if (typeof(T) == typeof(string))
            {
                GUI.contentColor = originalColor; // keep normal color for strings
                value = (T)(object)EditorGUI.TextField(fieldRect, (string)(object)value);
            }
            else
            {
                GUI.contentColor = originalColor;
                if (typeof(T) == typeof(int))
                    value = (T)(object)EditorGUI.IntField(fieldRect, (int)(object)value);
                else if (typeof(T) == typeof(float))
                    value = (T)(object)EditorGUI.FloatField(fieldRect, (float)(object)value);
            }

            callBack?.Invoke(value);

            // Reset color after drawing
            GUI.contentColor = originalColor;
        }

        private void DrawDropdownList(string label,
                                        ref int value,
                                        List<string> options,
                                        Action<string> callBack,
                                        string nullLabel = "<None>")
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new(rect.x, rect.y, Label_Width, rect.height);
            Rect fieldRect = new(rect.x + Label_Width + Spacing, rect.y, Field_Width, rect.height);

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
            if (value != previousValue)
            {
                if (value > 0)
                {
                    callBack.Invoke(options[value - 1]);
                }
                else
                {
                    callBack.Invoke(options[0]);
                }
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
                GUIStyle centeredStyle = new(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                    GUILayout.Label(
                        DoubleSize(_sprite),
                        centeredStyle,
                        GUILayout.Width(128),
                        GUILayout.Height(128)
                    );
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

        private static Texture2D DoubleSize(Texture2D source, int factor=2)
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

        private void FlipX()
        {
            static Texture2D FlipTextureByX(Texture2D original)
            {
                int width = original.width;
                int height = original.height;

                Texture2D flipped = new Texture2D(width, height, original.format, false);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Mirror vertically: pixel from (x, y) goes to (x, height - 1 - y)
                        flipped.SetPixel(x, height - 1 - y, original.GetPixel(x, y));
                    }
                }

                flipped.Apply();
                return flipped;
            }

            _curr.sprite = FlipTextureByX(_curr.sprite);
            _sprite = FlipTextureByX(_sprite);
        }

        private void FlipY()
        {
            static Texture2D FlipTextureByY(Texture2D original)
            {
                int width = original.width;
                int height = original.height;

                Texture2D flipped = new Texture2D(width, height, original.format, false);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Mirror horizontally: pixel from (x, y) goes to (width - 1 - x, y)
                        flipped.SetPixel(width - 1 - x, y, original.GetPixel(x, y));
                    }
                }

                flipped.Apply();
                return flipped;
            }
            _curr.sprite = FlipTextureByY(_curr.sprite);
            _sprite = FlipTextureByY(_sprite);
        }

        public void Update(SpriteButtonData data)
        {
            int animIndex = _animations.IndexOf(data.animation);
            if (animIndex >= 0)
                _selectedIndex = animIndex + 1;
            else
                _selectedIndex = 0;

            _sprite = data.sprite;
            _level = data.level;
            _order = data.order;
            _animation = data.animation;
            _name = data.label;
            _width = data.width;
            _height = data.height;
            _curr = data;
        }
    }
}