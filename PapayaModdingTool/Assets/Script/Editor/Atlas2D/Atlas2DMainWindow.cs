using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Shortcut;
using PapayaModdingTool.Assets.Script.Editor.Atlas2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Program;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Reader.Atlas2D;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D
{
    public class Atlas2DMainWindow : MainWindow
    {
        // private readonly Texture2D _previewTexture;
        private List<Texture2DButtonData> _texture2DButtonDatas;
        private List<SpriteButtonData> _allDatasInTexture;
        private List<SpriteButtonData> _workplace;
        private readonly TextureExporter _textureExporter = new(_appEnvironment);
        private readonly ProjectLoader _projectLoader = new();
        private WorkplaceExportor _workplaceExportor;

        private PreviewTexturePanel _previewPanel;
        private SpritesPanel _spritesPanel;
        private SpriteEditPanel _spriteEditPanel;
        private TexturesPanel _texturesPanel;
        private SpritesBatchSelector _batchSelector;
        private SpritesPanelSaver _spritesPanelSaver;
        private SpritesPanelReader _spritesPanelReader;

        private readonly CommandManager _spriteEditCommandManager = new();
        private readonly CommandManager _spritesPanelCommandManager = new();
        private ShortcutManagerEnabler _shortcutManagerEnabler;
        private ShortcutManager _spriteEditShortcutManager;
        private ShortcutManager _spritesPanelShortcutManager;

        private void InitPreviewPanel()
        {
            _previewPanel = new()
            {
                ELT = var => ELT(var),
                GetWorkplaceExportor = () => _workplaceExportor,
                GetDatas = () => _workplace
            };
            _previewPanel.Initialize(new(800, 10, 450, 900));
            // _previewPanel.SetPanOffset(new(0, _previewTexture != null ? -_previewTexture.height / 2f : 0f));

            _workplace = new();
            _workplaceExportor = new(_appEnvironment.Wrapper.JsonSerializer,
                                    _appEnvironment.Wrapper.FileBrowser,
                                    var => ELT(var));
        }

        private void InitSpritesPanel()
        {
            _spritesPanel = new()
            {
                ELT = var => ELT(var),
                GetSpriteButtonDataListener = () => _spriteEditPanel,
                GetFileFolderNameListener = () => _spriteEditPanel,
                GetAssetsManager = () => _appEnvironment.AssetsManager,
                GetTextureEncoderDecoder = () => _appEnvironment.Wrapper.TextureEncoderDecoder,
                GetDatas = () => _allDatasInTexture,
                SetDatas = var => _allDatasInTexture = var,
                GetBatchSelector = () => _batchSelector,
                GetSaver = () => _spritesPanelSaver,
                GetReader = () => _spritesPanelReader,
                GetProjectName = () => ProjectName,
                SetAnimations = var => _spriteEditPanel.SetAnimations(var),
                GetShortcutManager = () => _spritesPanelShortcutManager
            };
            _spritesPanel.Initialize(new(270, 20, 530, 520));

            _batchSelector ??= new()
            {
                GetDatas = () => _allDatasInTexture
            };
            _spritesPanelSaver = new(_appEnvironment.Wrapper.JsonSerializer);
            _spritesPanelReader = new(_appEnvironment.Wrapper.JsonSerializer);
        }

        private void InitSpriteEditPanel()
        {
            _spriteEditPanel = new()
            {
                ELT = var => ELT(var),
                GetDatas = () => _workplace,
                SetDatas = var =>
                {
                    _workplace = var;
                    _previewPanel.UpdateWorkplace(var);
                },
                GetAllDatasInTexture = () => _allDatasInTexture,
                GetBatchSelector = () => _batchSelector,
                GetCommandManager = () => _appEnvironment.CommandManager,
                GetProjectName = () => ProjectName,
                GetSaver = () => _spritesPanelSaver
            };
            _spriteEditPanel.Initialize(new(270, 550, 530, 360));

            _batchSelector ??= new()
            {
                GetDatas = () => _allDatasInTexture
            };
        }

        private void InitTexturesPanel()
        {
            _texturesPanel = new()
            {
                ELT = var => ELT(var),
                GetTexture2DButtonDatas = () => _texture2DButtonDatas,
                GetListener = () => _spritesPanel,
            };
            _texturesPanel.Initialize(new(10, 20, 250, 890));
            _texture2DButtonDatas = new();

            LoadTextureButtonDatas();
        }

        private void LoadTextureButtonDatas()
        {
            List<(string, string)> loaded = _projectLoader.FindLoadedPathAndFileFolderNameTextureOnly(ProjectName, _appEnvironment.Wrapper.JsonSerializer);
            List<string> fileFolderNames = _projectLoader.FindLoadedFileFolderNamesTextureOnly(ProjectName, _appEnvironment.Wrapper.JsonSerializer);
            BundleReader bundleReader = new(_appEnvironment.AssetsManager, _appEnvironment.Dispatcher);

            _texture2DButtonDatas = new();
            foreach ((string bundlePath, string fileFolderName) in loaded)
            {
                (BundleFileInstance _, AssetsFileInstance assetsInst) = bundleReader.ReadBundle(bundlePath);
                _texture2DButtonDatas.AddRange(ImageReader.ReadTexture2DButtonDatas(bundlePath,
                                                                            fileFolderName,
                                                                            assetsInst,
                                                                            _appEnvironment.AssetsManager,
                                                                            _textureExporter));
            }

            foreach (string fileFolderName in fileFolderNames)
            {
                string importedTexturesPath = string.Format(PredefinedPaths.ExternalFileTextureImportedFolder,
                                                        ProjectName,
                                                        fileFolderName);
                _texture2DButtonDatas.Add(new()
                {
                    sourcePath = importedTexturesPath,
                    fileFolderName = fileFolderName,
                    label = "Imported",
                    importedTexturesPath = importedTexturesPath
                });
            }

            // Sort
            _texture2DButtonDatas = _texture2DButtonDatas.OrderBy(o =>
            {
                var match = Regex.Match(o.label, @"\d+$");
                if (match.Success && int.TryParse(match.Value, out int num))
                    return num;
                else
                    return int.MaxValue; // no number → push to end
            })
            .ThenBy(o => o.label) // optional: sort alphabetically among "no-number" names
            .ToList();
        }

        private void InitShortcutManagers()
        {
            _shortcutManagerEnabler = new();
            _spriteEditShortcutManager = new(_spriteEditCommandManager);
            _spritesPanelShortcutManager = new(_spritesPanelCommandManager);
            _shortcutManagerEnabler.AddShortcutManager(_spriteEditShortcutManager);
            _shortcutManagerEnabler.AddShortcutManager(_spritesPanelShortcutManager);
            _spriteEditShortcutManager.AssignEnabler(_shortcutManagerEnabler);
            _spritesPanelShortcutManager.AssignEnabler(_shortcutManagerEnabler);
        }

        public static void Open(string projectPath)
        {
            var window = GetWindow<Atlas2DMainWindow>(Path.GetFileName(projectPath));
            window.Initialize(projectPath);
            window.Show();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            if (_previewPanel == null)
            {
                InitPreviewPanel();
            }
            if (_spritesPanel == null)
            {
                InitSpritesPanel();
            }
            if (_spriteEditPanel == null)
            {
                InitSpriteEditPanel();
            }
            if (_texturesPanel == null)
            {
                InitTexturesPanel();
            }
            if (_spriteEditShortcutManager == null)
            {
                InitShortcutManagers();
            }

            UEvent e = UEvent.current;
            _spriteEditShortcutManager.HandleEvent(e);
            _spritesPanelShortcutManager.HandleEvent(e);
            _previewPanel?.CreatePanel();
            _spritesPanel?.CreatePanel();
            _spriteEditPanel?.CreatePanel();
            _texturesPanel?.CreatePanel();
        }
    }
}