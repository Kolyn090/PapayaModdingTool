using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Shortcut;
using PapayaModdingTool.Assets.Script.Editor.Atlas2DMainHelper;
using PapayaModdingTool.Assets.Script.Misc.AppCore;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Program;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Reader.Atlas2D;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using PapayaModdingTool.Assets.Script.Reader.ProjectUtil;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D
{
    public class Atlas2DMain
    {
        private List<Texture2DButtonData> _texture2DButtonDatas;
        private List<SpriteButtonData> _allDatasInTexture;
        private List<SpriteButtonData> _workplace = new();
        private TextureExporter _textureExporter;
        private readonly ProjectLoader _projectLoader = new();

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

        public SpriteEditPanel SpriteEditPanel => _spriteEditPanel;
        public SpritesPanel SpritesPanel => _spritesPanel;
        public PreviewTexturePanel PreviewTexturePanel => _previewPanel;
        public TexturesPanel TexturesPanel => _texturesPanel;
        public ShortcutManager SpriteEditShortcutManager => _spriteEditShortcutManager;
        public ShortcutManager SpritesPanelShortcutManager => _spritesPanelShortcutManager;
        private bool _hasInit = false;
        public bool HasInit => _hasInit;

        public void Initialize(Func<string, string> ELT, AppEnvironment appEnvironment, string projectName)
        {
            _textureExporter = new(appEnvironment);
            _batchSelector ??= new(() => _allDatasInTexture);
            _spritesPanelSaver = new(appEnvironment.Wrapper.JsonSerializer);
            _spritesPanelReader = new(appEnvironment.Wrapper.JsonSerializer);

            LoadTextureButtonDatas(appEnvironment, projectName);

            InitPreviewPanel(ELT, appEnvironment);
            InitSpritesPanel(ELT, appEnvironment, projectName);
            InitSpriteEditPanel(ELT, projectName);
            InitTexturesPanel(ELT, appEnvironment, projectName);
            InitShortcutManagers();
            _hasInit = true;
        }

        private void InitPreviewPanel(Func<string, string> ELT, AppEnvironment appEnvironment)
        {
            _previewPanel = new(
                ELT,
                () => _workplace,
                new(appEnvironment.Wrapper.JsonSerializer,
                    appEnvironment.Wrapper.FileBrowser,
                    var => ELT(var))
            );
            _previewPanel.Initialize(new(800, 10, 450, 900));
        }

        private void InitSpritesPanel(Func<string, string> ELT, AppEnvironment appEnvironment, string projectName)
        {
            _spritesPanel = new(
                ELT,
                () => _allDatasInTexture,
                var => _allDatasInTexture = var,
                var => _spriteEditPanel.SetAnimations(var),
                appEnvironment.AssetsManager,
                appEnvironment.Wrapper.TextureEncoderDecoder,
                () => _spriteEditPanel,
                () => _spriteEditPanel,
                _batchSelector,
                _spritesPanelSaver,
                _spritesPanelReader,
                () => _spritesPanelShortcutManager,
                projectName
            );
            _spritesPanel.Initialize(new(270, 20, 530, 520));
        }

        private void InitSpriteEditPanel(Func<string, string> ELT, string projectName)
        {
            _spriteEditPanel = new(
                ELT,
                () => _allDatasInTexture,
                () => _workplace,
                var =>
                {
                    _workplace = var;
                    _previewPanel.UpdateWorkplace(var);
                },
                _spritesPanel.ForceReload,
                _batchSelector,
                _spriteEditCommandManager,
                _spritesPanelSaver,
                () => _spriteEditShortcutManager,
                projectName
            );
            _spriteEditPanel.Initialize(new(270, 550, 530, 360));
        }

        private void InitTexturesPanel(Func<string, string> ELT, AppEnvironment appEnvironment, string projectName)
        {
            _texturesPanel = new(
                ELT,
                _texture2DButtonDatas,
                _spritesPanel
            );
            _texturesPanel.Initialize(new(10, 20, 250, 890));
        }

        private void LoadTextureButtonDatas(AppEnvironment appEnvironment, string projectName)
        {
            List<(string, string)> loaded = _projectLoader.FindLoadedPathAndFileFolderNameTextureOnly(projectName, appEnvironment.Wrapper.JsonSerializer);
            List<string> fileFolderNames = _projectLoader.FindLoadedFileFolderNamesTextureOnly(projectName, appEnvironment.Wrapper.JsonSerializer);
            BundleReader bundleReader = new(appEnvironment.AssetsManager, appEnvironment.Dispatcher);

            _texture2DButtonDatas = new();
            foreach ((string bundlePath, string fileFolderName) in loaded)
            {
                (BundleFileInstance _, AssetsFileInstance assetsInst) = bundleReader.ReadBundle(bundlePath);
                _texture2DButtonDatas.AddRange(ImageReader.ReadTexture2DButtonDatas(bundlePath,
                                                                            fileFolderName,
                                                                            assetsInst,
                                                                            appEnvironment.AssetsManager,
                                                                            _textureExporter));
            }

            foreach (string fileFolderName in fileFolderNames)
            {
                string importedTexturesPath = string.Format(PredefinedPaths.ExternalFileTextureImportedFolder,
                                                        projectName,
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

            _spriteEditShortcutManager.AssignSavable(_spriteEditPanel);
            _spriteEditShortcutManager.AssignNavigable(_spriteEditPanel);
            _spritesPanelShortcutManager.AssignSavable(_spritesPanel);
            _spritesPanelShortcutManager.AssignNavigable(_spritesPanel);

            _spriteEditShortcutManager.AddCallOnShortcutDisable(_spriteEditPanel);
        }
    }
}