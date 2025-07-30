using AssetsTools.NET.Texture;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode
{
    public interface ITextureEncoder<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        byte[] Encode(Image<TPixel> image, int width, int height, TextureFormat format, int quality = 5, int mips = 1);
    }
}