using AssetsTools.NET.Texture;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode
{
    public interface ITextureDecoder
    {
        byte[] Decode(byte[] data, int width, int height, TextureFormat format);
        byte[] DecodeToBcBytes(byte[] imageBytes, int width, int height, TextureCompressionFormat format);
    }
}