using System;
using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Universal.GraphicUI;
using PapayaModdingTool.Assets.Script.EventListener;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper
{
    public class SpriteEditPanel : ISpriteButtonDataListener
    {
        private const float Field_Width = 140f; // width of the input box
        private const float Label_Width = 60f;  // width of the label
        private const float Spacing = 5f;      // space between label and field

        public Func<string, string> ELT;
        public Func<List<SpriteButtonData>> GetAllDatasInTexture;
        public Func<List<SpriteButtonData>> GetDatas; // Workplace
        public Func<SpritesBatchSelector> GetBatchSelector;
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
        private float _pivotX;
        private float _pivotY;
        private SpriteButtonData _curr;
        private readonly List<string> _animations = new();
        private int _selectedIndex = 0; // currently selected index
        private bool _hasInit;
        private SpritesBatchOperator _batchOperator;

        private Rect _bound;

        public void Initialize(Rect bound)
        {
            _bound = bound;
            _hasInit = true;

            _batchOperator = new()
            {
                GetData = GetAllDatasInTexture,
                GetDisplaySprite = () => _sprite,
                SetDisplaySprite = var => _sprite = var
            };
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
                    DrawField(ELT("sprite_edit_name"), ref _name, var => { if (_curr != null) _batchOperator.RenameSpriteLabel(var, _curr); });
                    DrawField(ELT("sprite_edit_level"), ref _level, var => { if (_curr != null) _batchOperator.ChangeLevelOfSelected(var); });
                    DrawField(ELT("sprite_edit_order"), ref _order, var => { if (_curr != null) _batchOperator.ChangeOrderOfSelected(var); });
                    DrawField(ELT("sprite_edit_width"), ref _width, var => { if (_curr != null) _batchOperator.ChangeWidthOfSelected(var); });
                    DrawField(ELT("sprite_edit_height"), ref _height, var => { if (_curr != null) _batchOperator.ChangeHeightOfSelected(var); });
                    DrawField(ELT("pivot_x"), ref _pivotX, var => { if (_curr != null) _batchOperator.ChangePivotXOfSelected(var); });
                    DrawField(ELT("pivot_y"), ref _pivotY, var => { if (_curr != null) _batchOperator.ChangePivotYOfSelected(var); });
                    DrawField(ELT("create_new_animation"), ref _newAnimation);
                    DrawField(ELT("delete_animation"), ref _deleteAnimation);
                    DrawDropdownList(ELT("sprite_edit_animation"),
                                        ref _selectedIndex,
                                        _animations,
                                        var => { _animation = var; if (_curr != null) _batchOperator.ChangeAnimationOfSelected(var); });
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin

                        if (GUILayout.Button(ELT("flip_x"), GUILayout.Width(100)))
                        {
                            _batchOperator.FlipXAllSelected();
                        }

                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(20); // left margin

                        if (GUILayout.Button(ELT("flip_y"), GUILayout.Width(100)))
                        {
                            _batchOperator.FlipYAllSelected();
                        }

                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label("");
                    GUILayout.Label("");
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
                            PlayAnimation();
                        }
                        GUILayout.Space(20); // right margin
                    }
                    GUILayout.EndHorizontal();

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

        private void PlayAnimation()
        {
            if (_selectedIndex != 0)
            {
                PlayAnimationPanel.Open(GetAllDatasInTexture().Where(x => x.animation == _animation).ToList());
            }
            else
            {
                Debug.LogWarning("No animation selected, cannot play animation. Abort.");
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

        private void DrawField<T>(string label, ref T value, Action<T> callBack = null)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new(rect.x, rect.y, Label_Width, rect.height);
            Rect fieldRect = new(rect.x + Label_Width + Spacing, rect.y, Field_Width, rect.height);

            EditorGUI.LabelField(labelRect, label);

            Color originalColor = GUI.contentColor;
            T newValue = value;

            EditorGUI.BeginChangeCheck(); // Start tracking changes

            if (typeof(T) == typeof(int))
            {
                int intValue = (int)(object)value;
                if (intValue < 0) GUI.contentColor = Color.red;
                intValue = EditorGUI.IntField(fieldRect, intValue);
                newValue = (T)(object)intValue;
            }
            else if (typeof(T) == typeof(float))
            {
                float floatValue = (float)(object)value;
                // if (floatValue < 0f) GUI.contentColor = Color.red;
                floatValue = EditorGUI.FloatField(fieldRect, floatValue);
                newValue = (T)(object)floatValue;
            }
            else if (typeof(T) == typeof(string))
            {
                string strValue = (string)(object)value;
                strValue = EditorGUI.TextField(fieldRect, strValue);
                newValue = (T)(object)strValue;
            }

            GUI.contentColor = originalColor;

            if (EditorGUI.EndChangeCheck()) // Only true if user actually changed the value
            {
                value = newValue;
                callBack?.Invoke(newValue);
            }
        }

        private void DrawDropdownList(string label,
                                        ref int value,
                                        List<string> options,
                                        Action<string> callBack,
                                        string nullLabel = "<None>")
        {
            string controlName = $"Dropdown_{label}";
            GUI.SetNextControlName(controlName);

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
            if (value != previousValue && EditorGUI.EndChangeCheck()) // check if really from user
            {
                if (value > 0)
                {
                    callBack.Invoke(options[value - 1]);
                }
                else
                {
                    callBack.Invoke("");
                }
                // Debug.Log(value > 0 ? $"Changed to {options[value-1]}" : $"Changed to None");
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
                // Define container (box) rect
                Rect containerRect = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false));

                // Draw the container background
                // EditorGUI.DrawRect(containerRect, new Color(0.1f, 0.1f, 0.1f, 1f));

                Texture2D doubled = DoubleSize(_sprite);

                // Compute rect for your sprite inside the container, centered
                Rect spriteRect = new(
                    containerRect.x + containerRect.width / 2 -  doubled.width / 2,  // center X
                    containerRect.y + containerRect.height / 2 - doubled.height / 2, // center Y
                    doubled.width,  // width
                    doubled.height  // height
                );

                // Draw sprite background
                // EditorGUI.DrawRect(spriteRect, new Color(1f, 0.2f, 0.2f, 1f));

                GUI.DrawTexture(spriteRect, doubled, ScaleMode.ScaleToFit, true);

                // Draw pivot on top
                PivotPoint.MakePivot(_pivotX, _pivotY, spriteRect);
            }
            else
            {
                GUILayout.Label("", GUILayout.ExpandHeight(true));
            }

            GUILayout.FlexibleSpace(); // push image up if vertical space
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); // right flexible space
            GUILayout.EndHorizontal();

            int selected = GetBatchSelector().GetNumOfSelected();
            string label;
            if (selected <= 1)
                label = !string.IsNullOrWhiteSpace(_name) ? _name : "<null>";
            else
                label = string.Format(ELT("num_selected"), selected);
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

            Texture2D result = new(width, height, source.format, false)
            {
                filterMode = FilterMode.Point // ensure point sampling
            };
            source.filterMode = FilterMode.Point;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Map to source coordinates using nearest integer
                    int srcX = x / factor;
                    int srcY = y / factor;
                    Color col = source.GetPixel(srcX, srcY);

                    // // !!! Debug
                    // // Replace fully transparent pixels with green
                    // if (col.a == 0f)
                    // {
                    //     col = Color.green;
                    // }

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
            _pivotX = data.pivot.x;
            _pivotY = data.pivot.y;
            _curr = data;
        }
    }
}