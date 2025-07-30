using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using PVRTexLib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UABS.Assets.Script.__Test__.UABEA.PVRTexLib
{
    public class PVRTexLibTester : MonoBehaviour
    {
        private void Start()
        {
            EncodeTexture("in.png", "out.bin", (ulong)PVRTexLibPixelFormat.PVRTCII_HDR_8bpp);
        }
        
        public static unsafe void EncodeTexture(string inFile, string outFile, ulong outFormat)
        {
            using (Image<Bgra32> image = Image.Load<Bgra32>(inFile)) // BGRA 8-8-8-8
            {
                int width = image.Width;
                int height = image.Height;

                if (!image.DangerousTryGetSinglePixelMemory(out var pixelMemory))
                    throw new InvalidOperationException("Unable to get pixel memory.");

                // Pin memory and get a raw pointer to pixel data
                using (var handle = pixelMemory.Pin())
                {
                    void* pixelPtr = handle.Pointer;

                    using (PVRTextureHeader header = new PVRTextureHeader(
                        PVRDefine.PVRTGENPIXELID4('b', 'g', 'r', 'a', 8, 8, 8, 8),
                        (uint)width,
                        (uint)height,
                        1, 1, 1, 1,
                        PVRTexLibColourSpace.sRGB,
                        PVRTexLibVariableType.UnsignedByteNorm,
                        false))
                    {
                        using (PVRTexture tex = new PVRTexture(header, pixelPtr))
                        {
                            if (tex.GetTextureDataSize() != 0)
                            {
                                if (tex.Transcode(outFormat, PVRTexLibVariableType.UnsignedByteNorm, PVRTexLibColourSpace.sRGB, 0, false))
                                {
                                    using (Stream outStream = File.Create(outFile))
                                    using (BinaryWriter bw = new BinaryWriter(outStream))
                                    {
                                        bw.Write(width);
                                        bw.Write(height);
                                        bw.Write(outFormat);
                                        bw.Write(new ReadOnlySpan<byte>(tex.GetTextureDataPointer(0), (int)tex.GetTextureDataSize(0)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}