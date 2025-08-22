using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Animation2D.Animation2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Animation2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;
using UnityEditor;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class Animation2DMainWindow : MainWindow
    {
        // private readonly Texture2D _previewTexture;
        private List<Texture2DButtonData> _texture2DButtonDatas;
        private List<SpriteButtonData> _allDatasInTexture;
        private List<SpriteButtonData> _workplace;
        private readonly TextureExporter _textureExporter = new(_appEnvironment);

        private PreviewTexturePanel _previewPanel;
        private SpritesPanel _spritesPanel;
        private SpriteEditPanel _spriteEditPanel;
        private TexturesPanel _texturesPanel;
        private SpritesBatchSelector _batchSelector;

        private void InitPreviewPanel()
        {
            _previewPanel = new()
            {
                ELT = var => ELT(var),
            };
            _previewPanel.Initialize(new(800, 10, 350, 800));
            // _previewPanel.SetPanOffset(new(0, _previewTexture != null ? -_previewTexture.height / 2f : 0f));

            _workplace = new();
        }

        private void InitSpritesPanel()
        {
            _spritesPanel = new()
            {
                ELT = var => ELT(var),
                GetListener = () => _spriteEditPanel,
                GetAssetsManager = () => _appEnvironment.AssetsManager,
                GetTextureEncoderDecoder = () => _appEnvironment.Wrapper.TextureEncoderDecoder,
                GetDatas = () => _allDatasInTexture,
                SetDatas = var => _allDatasInTexture = var,
                GetBatchSelector = () => _batchSelector
            };
            _spritesPanel.Initialize(new(270, 20, 530, 520));

            _batchSelector ??= new()
            {
                GetDatas = () => _allDatasInTexture
            };
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
                GetBatchSelector = () => _batchSelector
            };
            _spriteEditPanel.Initialize(new(270, 550, 530, 260));

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
                GetListener = () => _spritesPanel
            };
            _texturesPanel.Initialize(new(10, 20, 250, 790));
            _texture2DButtonDatas = new();

            // ! Make an example
            BundleReader bundleReader = new(_appEnvironment.AssetsManager, _appEnvironment.Dispatcher);
            string bundlePath = PathUtils.ToLongPath("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Otherworld Legends\\Otherworld Legends_Data\\StreamingAssets\\aa\\StandaloneWindows64\\unitspritesgroup_assets_assets\\sprites\\herounit\\hero_quanhuying\\unit_hero_quanhuying.psd_97f99a64c4a18168a8314aebe66b4d28.bundle");
            (BundleFileInstance bunInst, AssetsFileInstance assetsInst) = bundleReader.ReadBundle(bundlePath);
            _texture2DButtonDatas = ImageReader.ReadTexture2DButtonDatas(assetsInst,
                                                                        _appEnvironment.AssetsManager,
                                                                        _textureExporter);

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

            _texture2DButtonDatas.Add(new()
            {
                label = "Imported",
                importedTexturesPath = string.Format(PredefinedPaths.ExternalFileTextureImportedFolder,
                                                    "Quan_D-2.11.0.6",
                                                    "unit_hero_quanhuying_psd_97f99a64c4a18168a8314aebe66b4d28_bundle")
            });
        }

        public static void Open(string projectPath)
        {
            var window = GetWindow<Animation2DMainWindow>(Path.GetFileName(projectPath));
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

            _previewPanel?.CreatePanel();
            _spritesPanel?.CreatePanel();
            _spriteEditPanel?.CreatePanel();
            _texturesPanel?.CreatePanel();
        }
    }
}