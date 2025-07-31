using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AssetsTools.NET.Texture;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode
{
    public class BcDecoderWrapper : ITextureDecoder
    {
        private readonly BcDecoder _decoder = new();

        public byte[] Decode(byte[] data, int width, int height, TextureFormat format)
        {
            // ? There is no need to cover every format because they have
            // ? been taken of by other decoders.
            TextureCompressionFormat compressFormat = TextureCompressionFormat.Unknown;
            switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.BC4:
                    compressFormat = TextureCompressionFormat.Bc4;
                    break;
                case TextureFormat.DXT5:
                    compressFormat = TextureCompressionFormat.Bc3;
                    break;
                case TextureFormat.BC7:
                    compressFormat = TextureCompressionFormat.Bc7;
                    break;
                case TextureFormat.BC6H:
                    compressFormat = TextureCompressionFormat.Bc6U;
                    break;
                case TextureFormat.BC5:
                    compressFormat = TextureCompressionFormat.Bc5;
                    break;
            }

            if (compressFormat == TextureCompressionFormat.Unknown)
            {
                return null;
            }
            return DecodeToBytes(data, width, height, compressFormat);
        }

        public IColor[] DecodeRaw(byte[] imageBytes, int width, int height, TextureCompressionFormat format)
        {
            var decoded = _decoder.DecodeRaw(imageBytes, width, height, format.ToLibraryFormat());
            return decoded.Select(c => new ColorRgba32Wrapper(c) as IColor).ToArray();
        }

        public byte[] DecodeToBytes(byte[] imageBytes, int width, int height, TextureCompressionFormat format)
        {
            var libFormat = format.ToLibraryFormat();
            var decoded = _decoder.DecodeRaw(imageBytes, width, height, libFormat);
            var byteCount = decoded.Length * Unsafe.SizeOf<ColorRgba32>();
            var rgbaBytes = new byte[byteCount];
            MemoryMarshal.Cast<ColorRgba32, byte>(decoded.AsSpan()).CopyTo(rgbaBytes);
            return rgbaBytes;
        }
    }
}