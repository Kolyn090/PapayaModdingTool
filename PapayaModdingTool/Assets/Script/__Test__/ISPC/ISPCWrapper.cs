using System;
using System.Runtime.InteropServices;
using AssetsTools.NET.Texture;
using UABS.Assets.Script.__Test__;

namespace PapayaModdingTool.Assets.Script.__Test__
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RgbaSurface
    {
        public byte* ptr;
        public int width;
        public int height;
        public int stride;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct BC7EncSettings
    {
        public fixed bool mode_selection[4];
        public fixed int refineIterations[8];
        public bool skip_mode2;
        public int fastSkipTreshold_mode1;
        public int fastSkipTreshold_mode3;
        public int fastSkipTreshold_mode7;
        public int mode45_channel0;
        public int refineIterations_channel;
        public int channels;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BC6HEncSettings
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool slow_mode;
        [MarshalAs(UnmanagedType.I1)]
        public bool fast_mode;
        public int refineIterations_1p;
        public int refineIterations_2p;
        public int fastSkipTreshold;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ETCEncSettings
    {
        public int fastSkipTreshold;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ASTCEncSettings
    {
        public int block_width;
        public int block_height;
        public int channels;
        public int fastSkipTreshold;
        public int refineIterations;
    }

    public static class NativeTexCompressor
    {
        const string DLL = "libtexcomp";

        // BC7 Profiles
        [DllImport(DLL)] public static extern void GetProfile_ultrafast(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_veryfast(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_fast(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_basic(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_slow(ref BC7EncSettings settings);

        [DllImport(DLL)] public static extern void GetProfile_alpha_ultrafast(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_alpha_veryfast(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_alpha_fast(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_alpha_basic(ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_alpha_slow(ref BC7EncSettings settings);

        // BC6H Profiles
        [DllImport(DLL)] public static extern void GetProfile_bc6h_veryfast(ref BC6HEncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_bc6h_fast(ref BC6HEncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_bc6h_basic(ref BC6HEncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_bc6h_slow(ref BC6HEncSettings settings);
        [DllImport(DLL)] public static extern void GetProfile_bc6h_veryslow(ref BC6HEncSettings settings);

        // ETC Profile
        [DllImport(DLL)] public static extern void GetProfile_etc_slow(ref ETCEncSettings settings);

        // ASTC Profiles
        [DllImport(DLL)] public static extern void GetProfile_astc_fast(ref ASTCEncSettings settings, int block_width, int block_height);
        [DllImport(DLL)] public static extern void GetProfile_astc_alpha_fast(ref ASTCEncSettings settings, int block_width, int block_height);
        [DllImport(DLL)] public static extern void GetProfile_astc_alpha_slow(ref ASTCEncSettings settings, int block_width, int block_height);

        // Compression Functions
        [DllImport(DLL)] public static extern unsafe void CompressBlocksBC1(RgbaSurface* src, byte* dst);
        [DllImport(DLL)] public static extern unsafe void CompressBlocksBC3(RgbaSurface* src, byte* dst);
        [DllImport(DLL)] public static extern unsafe void CompressBlocksBC4(RgbaSurface* src, byte* dst);
        [DllImport(DLL)] public static extern unsafe void CompressBlocksBC5(RgbaSurface* src, byte* dst);
        [DllImport(DLL)] public static extern unsafe void CompressBlocksBC6H(RgbaSurface* src, byte* dst, ref BC6HEncSettings settings);
        [DllImport(DLL)] public static extern unsafe void CompressBlocksBC7(RgbaSurface* src, byte* dst, ref BC7EncSettings settings);
        [DllImport(DLL)] public static extern unsafe void CompressBlocksETC1(RgbaSurface* src, byte* dst, ref ETCEncSettings settings);
        [DllImport(DLL)] public static extern unsafe void CompressBlocksASTC(RgbaSurface* src, byte* dst, ref ASTCEncSettings settings);

        // Helper
        [DllImport(DLL)] public static extern unsafe void ReplicateBorders(RgbaSurface* dst, RgbaSurface* src, int x, int y, int bpp);
    }

    public static class ISPCWrapper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RgbaSurface
        {
            public IntPtr ptr;         // Points to BGRA32 pixel data
            public uint width;
            public uint height;
            public uint stride;
        }

        // Import native compression functions
        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressBlocksBC1(ref RgbaSurface surface, IntPtr outBuf);

        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressBlocksBC3(ref RgbaSurface surface, IntPtr outBuf);

        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressBlocksBC4(ref RgbaSurface surface, IntPtr outBuf);

        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressBlocksBC5(ref RgbaSurface surface, IntPtr outBuf);

        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetProfile_bc6h_basic(out Bc6hEncSettings settings);

        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressBlocksBC6H(ref RgbaSurface surface, IntPtr outBuf, ref Bc6hEncSettings settings);

        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetProfile_alpha_basic(out Bc7EncSettings settings);

        [DllImport("ispc_texcomp.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CompressBlocksBC7(ref RgbaSurface surface, IntPtr outBuf, ref Bc7EncSettings settings);

        // These must match the C struct layout
        [StructLayout(LayoutKind.Sequential)]
        public struct Bc6hEncSettings
        {
            // Fields must match bc6h_enc_settings in C
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Bc7EncSettings
        {
            // Fields must match bc7_enc_settings in C
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] data;
        }

        private static unsafe uint EncodeByISPC(IntPtr data, IntPtr outBuf, int mode, int level, uint width, uint height)
        {
            RgbaSurface surface = new RgbaSurface
            {
                ptr = data,
                width = width,
                height = height,
                stride = width * 4
            };

            int blockCountX = (int)((width + 3) >> 2);
            int blockCountY = (int)((height + 3) >> 2);
            int blockByteSize = 0;

            switch (mode)
            {
                case 10: // BC1 / DXT1
                    CompressBlocksBC1(ref surface, outBuf);
                    blockByteSize = 8;
                    break;
                case 12: // BC3 / DXT5
                    CompressBlocksBC3(ref surface, outBuf);
                    blockByteSize = 16;
                    break;
                case 26: // BC4
                    CompressBlocksBC4(ref surface, outBuf);
                    blockByteSize = 8;
                    break;
                case 27: // BC5
                    CompressBlocksBC5(ref surface, outBuf);
                    blockByteSize = 16;
                    break;
                case 24: // BC6H
                    Bc6hEncSettings bc6hsettings = new Bc6hEncSettings { data = new byte[80] };
                    GetProfile_bc6h_basic(out bc6hsettings);
                    CompressBlocksBC6H(ref surface, outBuf, ref bc6hsettings);
                    blockByteSize = 16;
                    break;
                case 25: // BC7
                    Bc7EncSettings bc7settings = new Bc7EncSettings { data = new byte[128] };
                    GetProfile_alpha_basic(out bc7settings);
                    CompressBlocksBC7(ref surface, outBuf, ref bc7settings);
                    blockByteSize = 16;
                    break;
                default:
                    return 0;
            }

            return (uint)(blockCountX * blockCountY * blockByteSize);
        }

        public static byte[] EncodeISPC(byte[] data, int width, int height, TextureFormat format, int quality=5)
        {
            int expectedSize = TextureEncoderDecoder.RGBAToFormatByteSize(format, width, height);
            byte[] dest = new byte[expectedSize];
            uint size = 0;
            unsafe
            {
                fixed (byte* dataPtr = data)
                fixed (byte* destPtr = dest)
                {
                    IntPtr dataIntPtr = (IntPtr)dataPtr;
                    IntPtr destIntPtr = (IntPtr)destPtr;
                    size = EncodeByISPC(dataIntPtr, destIntPtr, (int)format, quality, (uint)width, (uint)height);
                }
            }

            if (size > expectedSize)
            {
                throw new Exception($"ispc ({format}) encoded more data than expected!");
            }
            else if (size == expectedSize)
            {
                return dest;
            }
            else if (size > 0)
            {
                byte[] resizedDest = new byte[size];
                Buffer.BlockCopy(dest, 0, resizedDest, 0, (int)size);
                dest = null;
                return resizedDest;
            }
            else
            {
                dest = null;
                return null;
            }
        }
    }
}