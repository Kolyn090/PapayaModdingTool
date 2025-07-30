using AssetsTools.NET.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureUtil
{
    public interface ITextureImporter
    {
        byte[] Import(
            string imagePath, TextureFormat format,
            out int width, out int height, ref int mips,
            uint platform = 0, byte[] platformBlob = null);
    }

    public interface ITextureImporter<TPixel>: ITextureImporter where TPixel : unmanaged, IPixel<TPixel>
    {
        byte[] Import(
            Image<TPixel> image, TextureFormat format,
            out int width, out int height, ref int mips,
            uint platform = 0, byte[] platformBlob = null);
    }
}