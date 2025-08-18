using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Animation2D.Animation2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Animation2DMainHelper;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Misc.Paths;
using PapayaModdingTool.Assets.Script.Reader;
using PapayaModdingTool.Assets.Script.Reader.ImageDecoder;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D
{
    public class Animation2DMainWindow : MainWindow
    {
        private Texture2D _previewTexture;
        private List<SpriteButtonData> _spriteButtonDatas;
        private List<Texture2DButtonData> _texture2DButtonDatas;
        private readonly TextureExporter _textureExporter = new(_appEnvironment);

        private PreviewTexturePanel _previewPanel;
        private SpritesPanel _spritesPanel;
        private SpriteEditPanel _spriteEditPanel;
        private TexturesPanel _texturesPanel;

        private void InitPreviewPanel()
        {
            // ! Make an example
            BundleReader bundleReader = new(_appEnvironment.AssetsManager, _appEnvironment.Dispatcher);
            string bundlePath = PathUtils.ToLongPath("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Otherworld Legends\\Otherworld Legends_Data\\StreamingAssets\\aa\\StandaloneWindows64\\unitspritesgroup_assets_assets\\sprites\\herounit\\hero_quanhuying\\unit_hero_quanhuying.psd_97f99a64c4a18168a8314aebe66b4d28.bundle");
            (BundleFileInstance bunInst, AssetsFileInstance assetsInst) = bundleReader.ReadBundle(bundlePath);
            List<AssetFileInfo> texInfos = assetsInst.file.GetAssetsOfType(AssetClassID.Texture2D);
            _previewTexture = _textureExporter.ExportTextureWithPathIdAsTexture2D(assetsInst, texInfos[0]);

            // // Testing
            // byte[] imageData = File.ReadAllBytes(Path.Combine(
            // string.Format(PredefinedPaths.PapayaTextureProjectPath, "Quan_D-2.11.0.6"),
            // "unit_hero_quanhuying_psd_97f99a64c4a18168a8314aebe66b4d28_bundle",
            // "unit_hero_quanhuying_-992531485953202068.png"));
            // _previewTexture = new Texture2D(2, 2);
            // _previewTexture.LoadImage(imageData);
            // _previewTexture.Apply();
            _previewTexture.filterMode = FilterMode.Point;

            _previewPanel = new()
            {
                GetTexture = () => _previewTexture,
                ELT = var => ELT(var)
            };
            _previewPanel.Initialize(new(800, 10, 350, 800));
            _previewPanel.SetPanOffset(new(0, _previewTexture != null ? -_previewTexture.height / 2f : 0f));
        }

        private void InitSpritesPanel()
        {
            _spritesPanel = new()
            {
                ELT = var => ELT(var),
                GetSpriteButtonDatas = () => _spriteButtonDatas,
                GetListener = () => _spriteEditPanel
            };
            _spritesPanel.Initialize(new(270, 20, 530, 520));
            _spriteButtonDatas = new();

            // ! Make an example
            BundleReader bundleReader = new(_appEnvironment.AssetsManager, _appEnvironment.Dispatcher);
            string bundlePath = PathUtils.ToLongPath("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Otherworld Legends\\Otherworld Legends_Data\\StreamingAssets\\aa\\StandaloneWindows64\\unitspritesgroup_assets_assets\\sprites\\herounit\\hero_quanhuying\\unit_hero_quanhuying.psd_97f99a64c4a18168a8314aebe66b4d28.bundle");
            (BundleFileInstance bunInst, AssetsFileInstance assetsInst) = bundleReader.ReadBundle(bundlePath);
            _spriteButtonDatas = ImageReader.ReadSpriteButtonDatas(assetsInst,
                                                                    _appEnvironment.AssetsManager,
                                                                    _appEnvironment.Wrapper.TextureEncoderDecoder);
            _spriteButtonDatas = _spriteButtonDatas.OrderBy(o =>
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

        private void InitSpriteEditPanel()
        {
            _spriteEditPanel = new()
            {
                ELT = var => ELT(var),
            };
            _spriteEditPanel.Initialize(new(270, 550, 530, 260));
        }

        private void InitTexturesPanel()
        {
            _texturesPanel = new()
            {
                ELT = var => ELT(var),
                GetTexture2DButtonDatas = () => _texture2DButtonDatas
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