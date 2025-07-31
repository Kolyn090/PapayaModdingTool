using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;

namespace PapayaModdingTool.Assets.Script.Reader.TextureModding
{
    public class TextureAssetsLoader
    {
        private readonly AssetsManager _assetsManager;

        public TextureAssetsLoader(AssetsManager assetsManager)
        {
            _assetsManager = assetsManager;
        }

        public void LoadTextureAssets(LoadFileInfo loadInfo)
        {
            // Load the texture from bundle and save it to the correct place

            // Read dumps and save them as Source Dumps

            // Use dumps to slice the texture
        }
    }
}