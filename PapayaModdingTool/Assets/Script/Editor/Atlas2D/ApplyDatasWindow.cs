using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D
{
    public class ApplyDatasWindow : BaseEditorWindow
    {
        private Texture2D _targetTexture;
        private Texture2D _textureToReplace;
        private string _datasPath;
        private int _ppu = 100;

        [MenuItem("Tools/02 Apply Datas to Texture", false, 100)]
        public static void ShowWindow()
        {
            Initialize();
            GetWindow<ApplyDatasWindow>(ELT("apply_datas"));
        }

        private void OnGUI()
        {
            _ppu = EditorGUI.IntField(new Rect(10, 10, position.width - 20, EditorGUIUtility.singleLineHeight), ELT("ppu"), _ppu);

            GUILayout.BeginArea(new(10, 20 + EditorGUIUtility.singleLineHeight, position.width - 20, 250));

            _targetTexture = (Texture2D)EditorGUILayout.ObjectField("Texture.png", _targetTexture, typeof(Texture2D), false);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Datas.json", GUILayout.Width(80));
                _datasPath = GUILayout.TextField(_datasPath);
            }
            if (GUILayout.Button(ELT("browse"), GUILayout.Width(80)))
            {
                string[] paths = _appEnvironment.Wrapper.FileBrowser.OpenFilePanel("", "", false);
                if (paths.Length <= 0)
                {
                    Debug.Log("No path found. Abort.");
                }
                else
                {
                    _datasPath = paths[0];
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(_targetTexture == null || string.IsNullOrWhiteSpace(_datasPath));
            if (GUILayout.Button(ELT("run")))
            {
                Import();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(35);

            _textureToReplace = (Texture2D)EditorGUILayout.ObjectField(ELT("texture_to_replace"), _textureToReplace, typeof(Texture2D), false);

            EditorGUI.BeginDisabledGroup(_targetTexture == null || _textureToReplace == null);
            if (GUILayout.Button(ELT("replace_texture")))
            {
                ReplaceTexture();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndArea();
        }

        private void ReplaceTexture()
        {
            string targetTexturePath = AssetDatabase.GetAssetPath(_targetTexture);
            string replaceTexturePath = AssetDatabase.GetAssetPath(_textureToReplace);

            // Copy texture file
            File.Copy(targetTexturePath, replaceTexturePath, true);

            // Copy meta file too
            string targetMetaPath  = targetTexturePath + ".meta";
            string replaceMetaPath = replaceTexturePath + ".meta";
            if (File.Exists(targetMetaPath))
            {
                File.Copy(targetMetaPath, replaceMetaPath, true);
            }

            // Delete original asset (and its meta)
            AssetDatabase.DeleteAsset(targetTexturePath);

            // Refresh
            AssetDatabase.Refresh();
        }

        private void Import()
        {
            List<SpriteMetaData> metas = MetasFromDatas();
            SpriteMetaData[] newMetas = metas.ToArray();
            string assetPath = AssetDatabase.GetAssetPath(_targetTexture);
            TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti != null)
            {
                ti = AutoConfigureInspector(ti);
                ti.spriteImportMode = SpriteImportMode.Multiple;
                ti.spritesheet = newMetas;
                Debug.Log($"Current number of Sprites in the sheet: {metas.Count}.");
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                CleanMetaNameFileIdTable(assetPath);
            }
            else
            {
                Debug.LogError($"Texture not found in {_targetTexture}.");
            }
        }

        private TextureImporter AutoConfigureInspector(TextureImporter ti)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;

            TextureImporterSettings textureSettings = new();
            ti.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteGenerateFallbackPhysicsShape = false;

            ti.SetTextureSettings(textureSettings);
            ti.isReadable = true;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti = ChangeTexturePPU(ti);

            return ti;
        }

        private TextureImporter ChangeTexturePPU(TextureImporter ti)
        {
            ti.spritePixelsPerUnit = _ppu;
            return ti;
        }

        private List<SpriteMetaData> MetasFromDatas()
        {
            if (!File.Exists(_datasPath))
            {
                return null;
            }
            List<SpriteMetaData> result = new();

            string content = File.ReadAllText(_datasPath);
            List<IJsonObject> objs = _appEnvironment.Wrapper.JsonSerializer.DeserializeToArray(content);
            foreach (IJsonObject obj in objs)
            {
                result.Add(new()
                {
                    name = obj.GetString("label").ToString(),
                    rect = new(obj.GetInt("x"), obj.GetInt("y"), obj.GetInt("width"), obj.GetInt("height")),
                    pivot = new(obj.GetFloat("pivot_x"), obj.GetFloat("pivot_y")),
                    border = Vector4.zero,
                    alignment = (int)SpriteAlignment.Custom
                });
            }

            return result;
        }
        
        private static void CleanMetaNameFileIdTable(string assetPath)
        {
            string metaPath = assetPath + ".meta";

            if (!File.Exists(metaPath))
            {
                Debug.LogError("Meta file does not exist: " + metaPath);
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                Debug.LogError("TextureImporter not valid or not in Multiple mode.");
                return;
            }

            // Get current sprite names
            HashSet<string> validNames = new();
            foreach (var meta in importer.spritesheet)
            {
                validNames.Add(meta.name);
            }

            // Read and process meta file lines
            string[] lines = File.ReadAllLines(metaPath);
            List<string> newLines = new();
            bool insideTable = false;
            int removed = 0;

            foreach (var line in lines)
            {
                if (line.Trim() == "nameFileIdTable:")
                {
                    insideTable = true;
                    newLines.Add(line);
                    continue;
                }

                if (insideTable)
                {
                    Match match = Regex.Match(line, @"^\s+(.+?):\s*(-?\d+)");
                    if (match.Success)
                    {
                        string key = match.Groups[1].Value;
                        if (validNames.Contains(key))
                            newLines.Add(line);
                        else
                            removed++;
                    }
                    else if (line.StartsWith("  ")) // Still possibly inside block
                    {
                        newLines.Add(line);
                    }
                    else
                    {
                        insideTable = false;
                        newLines.Add(line);
                    }
                }
                else
                {
                    newLines.Add(line);
                }
            }

            File.WriteAllLines(metaPath, newLines);
            Debug.Log($"Cleaned {removed} dangling entries from nameFileIdTable in: {metaPath}");

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}