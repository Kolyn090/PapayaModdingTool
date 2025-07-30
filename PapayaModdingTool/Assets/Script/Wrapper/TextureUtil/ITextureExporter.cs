using AssetsTools.NET.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureUtil
{
    public interface ITextureExporter
    {
        bool Export(
            byte[] encData, string imagePath, int width, int height,
            TextureFormat format, uint platform = 0, byte[] platformBlob = null);
    }

    public interface ITextureExporter<TPixel>: ITextureExporter where TPixel : unmanaged, IPixel<TPixel>
    {
        Image<TPixel> Export(
            byte[] encData, int width, int height,
            TextureFormat format, uint platform = 0, byte[] platformBlob = null);
    }
}