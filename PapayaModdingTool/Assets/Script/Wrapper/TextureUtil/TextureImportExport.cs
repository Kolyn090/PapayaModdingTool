using System.IO;
using AssetsTools.NET.Texture;
using PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureUtil
{
    // From UABEA
    public class TextureImportExport : ITextureImporter<Rgba32>, ITextureExporter<Rgba32>
    {
        private readonly ITextureDecoder _decoder;
        private readonly ITextureEncoder<Rgba32> _encoder;

        public TextureImportExport(ITextureDecoder decoder, ITextureEncoder<Rgba32> encoder)
        {
            _decoder = decoder;
            _encoder = encoder;
        }

        public byte[] Import(
            string imagePath, TextureFormat format,
            out int width, out int height, ref int mips,
            uint platform = 0, byte[] platformBlob = null)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(imagePath);
            return Import(image, format, out width, out height, ref mips, platform, platformBlob);
        }

        public byte[] Import(
            Image<Rgba32> image, TextureFormat format,
            out int width, out int height, ref int mips,
            uint platform = 0, byte[] platformBlob = null)
        {
            // switch swizzle code does not support mipmaps yet (they're a bit special)
            if (platform == 38 && platformBlob != null && platformBlob.Length != 0)
            {
                return ImportSwitch(image, format, out width, out height, platformBlob);
            }

            width = image.Width;
            height = image.Height;

            // can't make mipmaps from this image
            if (mips > 1 && (width != height || !TextureHelper.IsPo2(width)))
            {
                mips = 1;
            }

            image.Mutate(i => i.Flip(FlipMode.Vertical));

            byte[] encData = _encoder.Encode(image, width, height, format, 5, mips);
            return encData;
        }

        private byte[] ImportSwitch(
            Image<Rgba32> image, TextureFormat format,
            out int width, out int height,
            byte[] platformBlob = null)
        {
            int paddedWidth, paddedHeight;

            width = image.Width;
            height = image.Height;

            format = GetCorrectedSwitchTextureFormat(format);
            int gobsPerBlock = Texture2DSwitchDeswizzler.GetSwitchGobsPerBlock(platformBlob);
            Size blockSize = Texture2DSwitchDeswizzler.TextureFormatToBlockSize(format);
            Size newSize = Texture2DSwitchDeswizzler.GetPaddedTextureSize(width, height, blockSize.Width, blockSize.Height, gobsPerBlock);
            paddedWidth = newSize.Width;
            paddedHeight = newSize.Height;

            image.Mutate(i => i.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.BoxPad,
                Position = AnchorPositionMode.BottomLeft,
                PadColor = Color.Fuchsia, // full alpha?
                Size = newSize
            }).Flip(FlipMode.Vertical));

            Image<Rgba32> swizzledImage = Texture2DSwitchDeswizzler.SwitchSwizzle(image, blockSize, gobsPerBlock);

            byte[] encData = _encoder.Encode(swizzledImage, paddedWidth, paddedHeight, format);
            return encData;
        }

        public bool Export(
            byte[] encData, string imagePath, int width, int height,
            TextureFormat format, uint platform = 0, byte[] platformBlob = null)
        {
            using Image<Rgba32> image = Export(encData, width, height, format, platform, platformBlob);
            if (image == null)
                return false;

            // SaveImageAtPath(image, imagePath);
            return true;
        }

        public Image<Rgba32> Export(
            byte[] encData, int width, int height,
            TextureFormat format, uint platform = 0, byte[] platformBlob = null)
        {
            if (platform == 38 && platformBlob != null && platformBlob.Length != 0)
            {
                return ExportSwitch(encData, width, height, format, platformBlob);
            }

            byte[] decData = _decoder.Decode(encData, width, height, format);
            if (decData == null)
                return null;

            Image<Rgba32> image = Image.LoadPixelData<Rgba32>(decData, width, height);
            image.Mutate(i => i.Flip(FlipMode.Vertical));

            //SaveImageAtPath(image, file);

            return image;
        }

        private Image<Rgba32> ExportSwitch(
            byte[] encData, int width, int height,
            TextureFormat format, byte[] platformBlob = null)
        {
            int originalWidth = width;
            int originalHeight = height;

            format = GetCorrectedSwitchTextureFormat(format);
            int gobsPerBlock = Texture2DSwitchDeswizzler.GetSwitchGobsPerBlock(platformBlob);
            Size blockSize = Texture2DSwitchDeswizzler.TextureFormatToBlockSize(format);
            Size newSize = Texture2DSwitchDeswizzler.GetPaddedTextureSize(width, height, blockSize.Width, blockSize.Height, gobsPerBlock);
            width = newSize.Width;
            height = newSize.Height;

            byte[] decData = _decoder.Decode(encData, width, height, format);
            if (decData == null)
                return null;

            Image<Rgba32> image = Image.LoadPixelData<Rgba32>(decData, width, height);

            image = Texture2DSwitchDeswizzler.SwitchUnswizzle(image, blockSize, gobsPerBlock);
            if (originalWidth != width || originalHeight != height)
            {
                image.Mutate(i => i.Crop(originalWidth, originalHeight));
            }

            image.Mutate(i => i.Flip(FlipMode.Vertical));

            return image;
        }

        public static void SaveImageAtPath(Image<Rgba32> image, string path)
        {
            string ext = Path.GetExtension(path);
            switch (ext)
            {
                case ".png":
                    image.SaveAsPng(path);
                    break;
                case ".tga":
                    var encoder = new TgaEncoder() { BitsPerPixel = TgaBitsPerPixel.Pixel32 };
                    image.SaveAsTga(path, encoder);
                    break;
            }
        }

        private static TextureFormat GetCorrectedSwitchTextureFormat(TextureFormat format)
        {
            // in older versions of unity, rgb24 has a platformblob which shouldn't
            // be possible. it turns out in this case, the image is just rgba32.
            if (format == TextureFormat.RGB24)
            {
                return TextureFormat.RGBA32;
            }
            else if (format == TextureFormat.BGR24)
            {
                return TextureFormat.BGRA32;
            }
            return format;
        }
    }
}