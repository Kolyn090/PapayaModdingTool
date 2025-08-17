using System.Runtime.InteropServices;

namespace PapayaModdingTool.Assets.Script.Reader.ImageDecoder
{
    public class AstcDecoderNative
    {
        [DllImport("astc_decoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DecodeASTC(
        byte[] astcData,
        int dataLength,
        int width,
        int height,
        byte[] outRgba,
        int blockX,
        int blockY);
    }
}