using System;
using PVRTexLib;
using UABS.Assets.Script.__Test__;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__.PVRTexLib
{
    public class PVRTexLibWrapper
    {
        public static bool GetPVRTexLibModes(int mode, out ulong pvrtlMode, out PVRTexLibVariableType pvrtlVarType)
        {
            // Default assignments
            pvrtlMode = 0;
            pvrtlVarType = PVRTexLibVariableType.UnsignedByteNorm;

            switch (mode)
            {
                case 5: pvrtlMode = PVRDefine.PVRTGENPIXELID4('a', 'r', 'g', 'b', 8, 8, 8, 8); break;
                case 14: pvrtlMode = PVRDefine.PVRTGENPIXELID4('b', 'g', 'r', 'a', 8, 8, 8, 8); break;
                case 4: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8); break;
                case 3: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 0, 8, 8, 8, 0); break;
                case 2: pvrtlMode = PVRDefine.PVRTGENPIXELID4('a', 'r', 'g', 'b', 4, 4, 4, 4); break;
                case 13: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 4, 4, 4, 4); break;
                case 7: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 0, 5, 6, 5, 0); break;
                case 1: pvrtlMode = PVRDefine.PVRTGENPIXELID4('a', 0, 0, 0, 8, 0, 0, 0); break;
                case 63: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 0, 0, 0, 8, 0, 0, 0); break;
                case 9: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 0, 0, 0, 16, 0, 0, 0); break;
                case 62: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 0, 0, 16, 16, 0, 0); break;
                case 15: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 0, 0, 0, 16, 0, 0, 0); break;
                case 16: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 0, 0, 16, 16, 0, 0); break;
                case 17: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 16, 16, 16, 16); break;
                case 18: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 0, 0, 0, 32, 0, 0, 0); break;
                case 19: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 0, 0, 32, 32, 0, 0); break;
                case 20: pvrtlMode = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 32, 32, 32, 32); break;

                case 41:
                case 42:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.EAC_R11;
                    break;

                case 43:
                case 44:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.EAC_RG11;
                    break;

                case 34:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ETC1;
                    break;

                case 45:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ETC2_RGB;
                    break;

                case 46:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ETC2_RGB_A1;
                    break;

                case 47:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ETC2_RGBA;
                    break;

                case 30:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.PVRTCI_2bpp_RGB;
                    break;

                case 31:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.PVRTCI_2bpp_RGBA;
                    break;

                case 32:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.PVRTCI_4bpp_RGB;
                    break;

                case 33:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.PVRTCI_4bpp_RGBA;
                    break;

                case 48:
                case 54:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ASTC_4x4;
                    break;

                case 49:
                case 55:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ASTC_5x5;
                    break;

                case 50:
                case 56:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ASTC_6x6;
                    break;

                case 51:
                case 57:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ASTC_8x8;
                    break;

                case 52:
                case 58:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ASTC_10x10;
                    break;

                case 53:
                case 59:
                    pvrtlMode = (ulong)PVRTexLibPixelFormat.ASTC_12x12; break;

                default:
                    return false; // Unsupported mode
            }

            // Set variable type depending on float formats
            switch (mode)
            {
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    pvrtlVarType = PVRTexLibVariableType.SignedFloat;
                    break;
                default:
                    pvrtlVarType = PVRTexLibVariableType.UnsignedByteNorm;
                    break;
            }

            return true;
        }

        public static PVRTexLibCompressorQuality GetPVRTexLibCompressionLevel(ulong pvrtlMode)
        {
            switch (pvrtlMode)
            {
                case (ulong)PVRTexLibPixelFormat.PVRTCI_2bpp_RGB:
                case (ulong)PVRTexLibPixelFormat.PVRTCI_2bpp_RGBA:
                case (ulong)PVRTexLibPixelFormat.PVRTCI_4bpp_RGB:
                case (ulong)PVRTexLibPixelFormat.PVRTCI_4bpp_RGBA:
                case (ulong)PVRTexLibPixelFormat.PVRTCII_2bpp:
                case (ulong)PVRTexLibPixelFormat.PVRTCII_4bpp:
                    return PVRTexLibCompressorQuality.PVRTCNormal;

                case (ulong)PVRTexLibPixelFormat.ETC1:
                case (ulong)PVRTexLibPixelFormat.ETC2_RGB:
                case (ulong)PVRTexLibPixelFormat.ETC2_RGBA:
                case (ulong)PVRTexLibPixelFormat.ETC2_RGB_A1:
                case (ulong)PVRTexLibPixelFormat.EAC_R11:
                case (ulong)PVRTexLibPixelFormat.EAC_RG11:
                    return PVRTexLibCompressorQuality.ETCNormal;

                case (ulong)PVRTexLibPixelFormat.ASTC_4x4:
                case (ulong)PVRTexLibPixelFormat.ASTC_5x4:
                case (ulong)PVRTexLibPixelFormat.ASTC_5x5:
                case (ulong)PVRTexLibPixelFormat.ASTC_6x5:
                case (ulong)PVRTexLibPixelFormat.ASTC_6x6:
                case (ulong)PVRTexLibPixelFormat.ASTC_8x5:
                case (ulong)PVRTexLibPixelFormat.ASTC_8x6:
                case (ulong)PVRTexLibPixelFormat.ASTC_8x8:
                case (ulong)PVRTexLibPixelFormat.ASTC_10x5:
                case (ulong)PVRTexLibPixelFormat.ASTC_10x6:
                case (ulong)PVRTexLibPixelFormat.ASTC_10x8:
                case (ulong)PVRTexLibPixelFormat.ASTC_10x10:
                case (ulong)PVRTexLibPixelFormat.ASTC_12x10:
                case (ulong)PVRTexLibPixelFormat.ASTC_12x12:
                case (ulong)PVRTexLibPixelFormat.ASTC_3x3x3:
                case (ulong)PVRTexLibPixelFormat.ASTC_4x3x3:
                case (ulong)PVRTexLibPixelFormat.ASTC_4x4x3:
                case (ulong)PVRTexLibPixelFormat.ASTC_4x4x4:
                case (ulong)PVRTexLibPixelFormat.ASTC_5x4x4:
                case (ulong)PVRTexLibPixelFormat.ASTC_5x5x4:
                case (ulong)PVRTexLibPixelFormat.ASTC_5x5x5:
                case (ulong)PVRTexLibPixelFormat.ASTC_6x5x5:
                case (ulong)PVRTexLibPixelFormat.ASTC_6x6x5:
                case (ulong)PVRTexLibPixelFormat.ASTC_6x6x6:
                    return PVRTexLibCompressorQuality.ASTCMedium;

                default:
                    return PVRTexLibCompressorQuality.PVRTCNormal;
            }
        }

        public static unsafe uint DecodeByPVRTexLib(IntPtr data, IntPtr outBuf, int mode, uint width, uint height)
        {
            if (!GetPVRTexLibModes(mode, out ulong pvrtlMode, out PVRTexLibVariableType pvrtlVarType))
            {
                Debug.Log("GetPVRTexLibModes failed");
                return 0;
            }

            ulong RGBA8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);

            var pvrth = new PVRTextureHeader(
                pvrtlMode, width, height,
                1, 1, 1, 1,
                PVRTexLibColourSpace.sRGB,
                pvrtlVarType
            );

            var pvrt = new PVRTexture(pvrth, (void*)data);

            bool success = pvrt.Transcode(
                RGBA8888,
                PVRTexLibVariableType.UnsignedByteNorm,
                PVRTexLibColourSpace.sRGB,
                PVRTexLibCompressorQuality.PVRTCNormal,
                false
            );

            if (!success)
            {
                Debug.Log("Transcode failed");
                return 0;
            }

            void* newData = pvrt.GetTextureDataPointer();
            uint size = (uint)pvrt.GetTextureDataSize();

            if (newData == null || size == 0)
            {
                Debug.Log("No decoded data");
                return 0;
            }

            Buffer.MemoryCopy(newData, (void*)outBuf, size, size);
            return size;
        }

        private static unsafe uint EncodeByPVRTexLib(IntPtr data, IntPtr outBuf, int mode, int level, uint width, uint height)
        {
            if (!GetPVRTexLibModes(mode, out ulong pvrtlMode, out PVRTexLibVariableType pvrtlVarType))
            {
                Debug.Log("EncodeByPVRTexLib failed");
                return 0;
            }

            PVRTexLibCompressorQuality compLevel = GetPVRTexLibCompressionLevel(pvrtlMode);

            ulong RGBA8888 = PVRDefine.PVRTGENPIXELID4('r', 'g', 'b', 'a', 8, 8, 8, 8);

            var pvrth = new PVRTextureHeader(RGBA8888, width, height);

            var pvrt = new PVRTexture(pvrth, (void*)data);

            bool success = pvrt.Transcode(pvrtlMode, pvrtlVarType, PVRTexLibColourSpace.sRGB, compLevel, false);

            if (!success)
            {
                return 0;
            }

            void* newData = (void*)pvrt.GetTextureDataPointer();
            uint size = (uint)pvrt.GetTextureDataSize();

            if (newData == null || size == 0)
                return 0;

            Buffer.MemoryCopy(newData, (void*)outBuf, size, size);

            return size;
        }

        public static byte[] EncodePVRTexLib(byte[] data, int width, int height, TextureFormat format, int quality)
        {
            int expectedSize = TextureEncoderDecoder.RGBAToFormatByteSize((AssetsTools.NET.Texture.TextureFormat)format, width, height);
            byte[] dest = new byte[expectedSize];
            uint size = 0;
            unsafe
            {
                fixed (byte* dataPtr = data)
                fixed (byte* destPtr = dest)
                {
                    IntPtr dataIntPtr = (IntPtr)dataPtr;
                    IntPtr destIntPtr = (IntPtr)destPtr;
                    size = EncodeByPVRTexLib(dataIntPtr, destIntPtr, (int)format, quality, (uint)width, (uint)height);
                }
            }

            if (size > expectedSize)
            {
                throw new Exception($"pvrtexlib ({format}) encoded more data than expected!");
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