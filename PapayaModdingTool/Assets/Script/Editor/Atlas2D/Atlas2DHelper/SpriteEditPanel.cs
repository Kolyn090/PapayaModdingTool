using System;
using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Shortcut;
using PapayaModdingTool.Assets.Script.Editor.Universal.GraphicUI;
using PapayaModdingTool.Assets.Script.EventListener;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Program;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper
{
    public class SpriteEditPanel : ISpriteButtonDataListener, IFileFolderNameListener, IShortcutSavable, IShortcutNavigable, ICallOnShortcutDisabled
    {
        private const float Field_Width = 140f; // width of the input box
        private const float Label_Width = 60f;  // width of the label
        private const float Spacing = 5f;      // space between label and field

        public Func<string, string> ELT;
        public Func<List<SpriteButtonData>> GetAllDatasInTexture;
        public Func<List<SpriteButtonData>> GetDatas; // Workplace
        public Func<SpritesBatchSelector> GetBatchSelector;
        public Func<CommandManager> GetCommandManager;
        public Func<string> GetProjectName;
        public Func<SpritesPanelSaver> GetSaver;
        public Func<ShortcutManager> GetShortcutManager;
        public Action<List<SpriteButtonData>> SetDatas; // Workplace
        public Action ForceUpdateSpritesPanel;

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
        private List<string> _animations = new();
        public void SetAnimations(List<string> animations)
        {
            _animations = animations;
        }
        private int _selectedIndex = 0; // currently selected index
        private bool _hasInit;
        private SpritesBatchOperator _batchOperator;
        private string _fileFolderName;
        private string GetJsonSavePath => string.Format(PredefinedPaths.Atlas2DSpritesPanelSaveJson,
                                                        GetProjectName(),
                                                        _fileFolderName);

        private Rect _bound;
        private readonly Dictionary<int, string> _controlNames = new()
        {
            {0, "sprite_edit_name"},
            {1, "sprite_edit_level"},
            {2, "sprite_edit_order"},
            {3, "sprite_edit_width"},
            {4, "sprite_edit_height"},
            {5, "pivot_x"},
            {6, "pivot_y"},
            {7, "create_new_animation"},
            {8, "delete_animation"},
            {9, "sprite_edit_animation"},
        };

        #region Optimization
        // For dropdown list
        private readonly Dictionary<List<string>, string[]> _cachedOptionArrays = new();
        // For sprite display
        private Texture2D _cachedDoubledSprite;
        private Texture2D _lastSprite;
        #endregion

        public void Initialize(Rect bound)
        {
            _bound = bound;
            _hasInit = true;

            _batchOperator = new()
            {
                GetDatas = GetAllDatasInTexture,
                GetDisplaySprite = () => _sprite,
                SetDisplaySprite = var => _sprite = var,
                GetCommandManager = GetCommandManager
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
                    DrawString(_controlNames[0], ref _name);
                    DrawIntRedTextIfNegative(_controlNames[1], ref _level);
                    DrawIntRedTextIfNegative(_controlNames[2], ref _order);
                    DrawIntRedTextIfNegative(_controlNames[3], ref _width);
                    DrawIntRedTextIfNegative(_controlNames[4], ref _height);
                    DrawFloat(_controlNames[5], ref _pivotX);
                    DrawFloat(_controlNames[6], ref _pivotY);
                    DrawString(_controlNames[7], ref _newAnimation);
                    DrawString(_controlNames[8], ref _deleteAnimation);
                    DrawDropdownList(_controlNames[9], ref _selectedIndex, _animations);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    DrawSideButton("save_changed", SaveChanged);
                    DrawSideButton("auto_fill_workplace", AutoFillWorkplace);
                    GUILayout.Label("");
                    GUILayout.Label("");
                    GUILayout.Label("");
                    DrawSideButton2("-0.1", "+0.1",
                        () => _batchOperator.AddPivotOfSelected(
                                () => _curr,
                                newPivotX => _pivotX = newPivotX,
                                newPivotY => _pivotY = newPivotY,
                                addX: -0.1f
                            ),
                        () => _batchOperator.AddPivotOfSelected(
                                () => _curr,
                                newPivotX => _pivotX = newPivotX,
                                newPivotY => _pivotY = newPivotY,
                                addX: 0.1f
                            )
                    );
                    DrawSideButton2("-0.1", "+0.1",
                        () => _batchOperator.AddPivotOfSelected(
                                () => _curr,
                                newPivotX => _pivotX = newPivotX,
                                newPivotY => _pivotY = newPivotY,
                                addY: -0.1f
                            ),
                        () => _batchOperator.AddPivotOfSelected(
                                () => _curr,
                                newPivotX => _pivotX = newPivotX,
                                newPivotY => _pivotY = newPivotY,
                                addY: 0.1f
                            )
                    );
                    DrawSideButton("sprite_edit_create", AddAnimation);
                    DrawSideButton("sprite_edit_delete", DeleteAnimation);
                    DrawSideButton("play_animation", PlayAnimation);

                    GUILayout.Label("");
                    DrawSideButton("move_to_trashbin", MoveSpriteToTrashBin);
                    DrawSideButton("undo_trashbin", UndoTrashbin);
                    DrawSideButton("duplicate", DuplicateSprite);
                    DrawSideButton("flip_x", _batchOperator.FlipXAllSelected);
                    DrawSideButton("flip_y", _batchOperator.FlipYAllSelected);
                    DrawSideButton("update_workplace", UpdateWorkplace, height: 40);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            GUILayout.EndArea();
        }

        // !!! Only for Imported
        private void MoveSpriteToTrashBin()
        {
            _batchOperator.MoveSelectedToTrashbin(GetProjectName(), _fileFolderName);
        }

        // !!! Only for Imported
        private void UndoTrashbin()
        {
            _batchOperator.UndoTrashbinForSelected(GetProjectName(), _fileFolderName);
        }

        // !!! Only for Imported
        private void DuplicateSprite()
        {
            bool confirm = EditorUtility.DisplayDialog(
                ELT("duplicate_sprite"),
                string.Format(ELT("duplicate_sprite_alert"), _batchOperator.Selected.Count),
                ELT("confirm"),
                ELT("cancel")
            );
            if (!confirm)
                return;

            _batchOperator.DuplicateSelected(GetProjectName(), _fileFolderName, GetJsonSavePath, GetSaver());
            ForceUpdateSpritesPanel();
        }

        private void AutoFillWorkplace()
        {
            _batchOperator.AutoFillWorkplace();
            _level = _curr.level;
            _order = _curr.order;
        }

        private void SaveChanged()
        {
            // For optimization purpose, only truly save to data if this button is clicked
            // Only update if the value has been changed

            if (_curr.label != _name)
                _batchOperator.RenameSpriteLabel(_name, () => _curr, newVal => _name = newVal);

            if (_curr.level != _level)
                _batchOperator.ChangeLevelOfSelected(_level, () => _curr, newVal => _level = newVal);

            if (_curr.order != _order)
                _batchOperator.ChangeOrderOfSelected(_order, () => _curr, newVal => _order = newVal);

            if (_curr.width != _width)
                _batchOperator.ChangeWidthOfSelected(_width, () => _curr, newVal => _width = newVal);

            if (_curr.height != _height)
                _batchOperator.ChangeHeightOfSelected(_height, () => _curr, newVal => _height = newVal);

            if (_curr.pivot.x != _pivotX)
                _batchOperator.ChangePivotXOfSelected(_pivotX, () => _curr, newVal => _pivotX = newVal);

            if (_curr.pivot.y != _pivotY)
                _batchOperator.ChangePivotYOfSelected(_pivotY, () => _curr, newVal => _pivotY = newVal);

            if (GetSelectIndexOfAnimation(_curr.animation) != _selectedIndex)
                _batchOperator.ChangeAnimationOfSelected(_selectedIndex, () => _curr,
                    newIndex => _selectedIndex = newIndex,
                    () => _animations,
                    newAnimation => _animation = newAnimation
            );

            Debug.Log("Memory change saved, you still need to Save everything to save to database.");
        }

        private int GetSelectIndexOfAnimation(string animation)
        {
            return _animations.IndexOf(animation) + 1;
        }

        private void AddAnimation()
        {
            if (string.IsNullOrWhiteSpace(_newAnimation))
            {
                Debug.LogWarning("Warning: New animation is empty. Abort creation.");
            }
            else if (_newAnimation == "<None>")
            {
                Debug.LogWarning("Warning: Bad name <None>. Abort.");
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
                    // SetDatas(GetDatas());
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
                    // SetDatas(copy);
                }
                // Add all valid sprites to workplace
                List<SpriteButtonData> validSprites = GetAllDatasInTexture().Where(x => x.level >= 0 && x.order >= 0).ToList();
                SetDatas(validSprites);
            }
        }

        private void DrawIntRedTextIfNegative(string controlName, ref int value)
        {
            (Rect labelRect, Rect fieldRect) = GetFieldRect();
            EditorGUI.LabelField(labelRect, ELT(controlName));
            GUI.SetNextControlName(controlName);
            if (value < 0)
            {
                Color originalColor = GUI.contentColor;
                GUI.contentColor = Color.red;
                int newValue = EditorGUI.IntField(fieldRect, value);
                GUI.contentColor = originalColor;
                if (newValue != value)
                    value = newValue;
            }
            else
            {
                int newValue = EditorGUI.IntField(fieldRect, value);
                if (newValue != value)
                    value = newValue;
            }

            // Check if this control is focused
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                FocusPanel();
            }
        }

        private void DrawFloat(string controlName, ref float value)
        {
            (Rect labelRect, Rect fieldRect) = GetFieldRect();
            EditorGUI.LabelField(labelRect, ELT(controlName));
            GUI.SetNextControlName(controlName);
            float newValue = EditorGUI.FloatField(fieldRect, value);
            if (newValue != value)
                value = newValue;
            
            // Check if this control is focused
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                FocusPanel();
            }
        }

        private void DrawString(string controlName, ref string value)
        {
            (Rect labelRect, Rect fieldRect) = GetFieldRect();
            EditorGUI.LabelField(labelRect, ELT(controlName));
            GUI.SetNextControlName(controlName);
            string newValue = EditorGUI.TextField(fieldRect, value);
            if (newValue != value)
                value = newValue;

            // Check if this control is focused
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                FocusPanel();
            }
        }

        private void DrawDropdownList(string controlName,
                                        ref int value,
                                        List<string> options,
                                        string nullLabel = "<None>")
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new(rect.x, rect.y, Label_Width, rect.height);
            Rect fieldRect = new(rect.x + Label_Width + Spacing, rect.y, Field_Width, rect.height);

            EditorGUI.LabelField(labelRect, ELT(controlName));

            // Cache or rebuild the array
            if (!_cachedOptionArrays.TryGetValue(options, out string[] optionArray) ||
                optionArray.Length != options.Count + 1 || optionArray[0] != nullLabel)
            {
                optionArray = new string[options.Count + 1];
                optionArray[0] = nullLabel;
                for (int i = 0; i < options.Count; i++)
                    optionArray[i + 1] = options[i];

                _cachedOptionArrays[options] = optionArray;
            }
            GUI.SetNextControlName(controlName);
            value = EditorGUI.Popup(fieldRect, value, optionArray);

            // Check if this control is focused
            if (GUI.GetNameOfFocusedControl() == controlName)
            {
                FocusPanel();
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

                if (_sprite != _lastSprite)
                {
                    _cachedDoubledSprite = DoubleSize(_sprite);
                    _lastSprite = _sprite;
                }

                // Compute rect for your sprite inside the container, centered
                Rect spriteRect = new(
                    containerRect.x + containerRect.width / 2 - _cachedDoubledSprite.width / 2,  // center X
                    containerRect.y + containerRect.height / 2 - _cachedDoubledSprite.height / 2, // center Y
                    _cachedDoubledSprite.width,  // width
                    _cachedDoubledSprite.height  // height
                );

                // Draw sprite background
                // EditorGUI.DrawRect(spriteRect, new Color(1f, 0.2f, 0.2f, 1f));

                GUI.DrawTexture(spriteRect, _cachedDoubledSprite, ScaleMode.ScaleToFit, true);

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

        private void DrawSideButton(string controlName, Action onClick, int width = 100, int height = 18)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(20); // left margin

                if (GUILayout.Button(ELT(controlName), GUILayout.Width(width), GUILayout.Height(height)))
                {
                    onClick.Invoke();
                    FocusPanel();
                }

                GUILayout.Space(20); // right margin
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSideButton2(string controlName1, string controlName2,
                                    Action onClick1, Action onClick2,
                                    int width = 40, int height = 20)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(20); // left margin

                if (GUILayout.Button(ELT(controlName1), GUILayout.Width(width), GUILayout.Height(height)))
                {
                    onClick1.Invoke();
                    FocusPanel();
                }

                GUILayout.Space(5);

                if (GUILayout.Button(ELT(controlName2), GUILayout.Width(width), GUILayout.Height(height)))
                {
                    onClick2.Invoke();
                    FocusPanel();
                }

                GUILayout.Space(20); // right margin
            }
            GUILayout.EndHorizontal();
        }

        // Do not store rect in a dictionary as it might be slower to do
        private (Rect, Rect) GetFieldRect()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect labelRect = new(rect.x, rect.y, Label_Width, rect.height);
            Rect fieldRect = new(rect.x + Label_Width + Spacing, rect.y, Field_Width, rect.height);
            return (labelRect, fieldRect);
        }

        private static Texture2D DoubleSize(Texture2D source, int factor = 2)
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
            _sprite = data.sprite;
            _level = data.level;
            _order = data.order;

            if (!string.IsNullOrWhiteSpace(data.animation) && !_animations.Contains(data.animation))
            {
                _animations.Add(data.animation);
            }
            _animation = data.animation;
            int animIndex = _animations.IndexOf(data.animation);
            if (animIndex >= 0)
                _selectedIndex = animIndex + 1;
            else
                _selectedIndex = 0;

            _name = data.label;
            _width = data.width;
            _height = data.height;
            _pivotX = data.pivot.x;
            _pivotY = data.pivot.y;
            _curr = data;
        }

        public void Update(string fileFolderName)
        {
            _fileFolderName = fileFolderName;
        }

        // Call when this panel is focused
        private void FocusPanel()
        {
            GetShortcutManager().IsEnabled = true;
        }

        // Call when Ctrl + S is clicked
        public void OnShortcutSave()
        {
            if (GetAllDatasInTexture == null || GetAllDatasInTexture() == null)
                return;

            void WriteToDb()
            {
                string importedPath = string.Format(PredefinedPaths.ExternalFileTextureImportedFolder, GetProjectName(), _fileFolderName);
                GetSaver().Save(GetJsonSavePath, importedPath, GetAllDatasInTexture());
            }
            SaveChanged();
            UpdateWorkplace();
            WriteToDb();
        }

        // Call when Arrow key is clicked
        public void OnShortcutNavigate(KeyCode keyCode)
        {
            string currFocus = GUI.GetNameOfFocusedControl();
            if (!_controlNames.ContainsValue(currFocus))
            {
                return;
            }
            int index = _controlNames.FirstOrDefault(x => x.Value == currFocus).Key;

            if (keyCode == KeyCode.UpArrow)
            {
                if (index > 0)
                {
                    GUI.FocusControl(_controlNames[index - 1]);
                }
                else
                {
                    GUI.FocusControl(_controlNames[_controlNames.Count-1]);
                }
            }
            else if (keyCode == KeyCode.DownArrow)
            {
                if (index < _controlNames.Count-1)
                {
                    GUI.FocusControl(_controlNames[index + 1]);
                }
                else
                {
                    GUI.FocusControl(_controlNames[0]);
                }
            }
        }

        public void OnShortcutDisabled()
        {
            GUI.FocusControl("");
        }
    }
}