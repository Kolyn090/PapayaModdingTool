using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Reader.ImageDecoder
{
    public class ImageReader
    {
        // ? Assume no atlas for now
        public static List<Texture2D> ReadSprites(AssetsFileInstance assetsFileInst,
                                                AssetsManager assetsManager,
                                                ITextureDecoder textureDecoder)
        {
            List<Texture2D> result = new();
            List<AssetFileInfo> spriteInfos = assetsFileInst.file.AssetInfos;
            result = spriteInfos.Select(x => SpriteToImage(assetsFileInst, x, assetsManager, textureDecoder)).Where(x => x != null).ToList();

            return result;
        }

        private static Texture2D SpriteToImage(AssetsFileInstance assetsFileInst,
                                            AssetFileInfo assetFileInfo,
                                            AssetsManager assetsManager,
                                            ITextureDecoder textureDecoder)
        {
            AssetTypeValueField spriteBase = assetsManager.GetBaseField(assetsFileInst, assetFileInfo);

            if (spriteBase["m_Rect"].IsDummy)
                return null;

            Rect spriteRect = new(
                spriteBase["m_Rect"]["x"].AsFloat,
                spriteBase["m_Rect"]["y"].AsFloat,
                spriteBase["m_Rect"]["width"].AsFloat,
                spriteBase["m_Rect"]["height"].AsFloat
            );

            if (spriteBase.Get("m_AtlasTags") != null)
            {
                if (spriteBase["m_AtlasTags"]["Array"].AsArray.size != 0)
                {
                    Debug.Log("No SpriteAtlas file found in bundle but Sprite has atlas tag. Skip.");
                    return null;
                }
            }

            AssetTypeValueField texRefField = spriteBase["m_RD"]["texture"];
            AssetExternal texAsset = GetExternalAsset(assetsFileInst,
                                                        assetsFileInst.parentBundle,
                                                        texRefField,
                                                        assetsManager);
            AssetTypeValueField texBase = assetsManager.GetBaseField(texAsset.file, texAsset.info);

            return ExtractImage(texBase, assetsFileInst, assetsFileInst.parentBundle, spriteRect, textureDecoder);
        }

        private static AssetExternal GetExternalAsset(AssetsFileInstance currentFile,
                                                BundleFileInstance bundleFile,
                                                AssetTypeValueField pptr,
                                                AssetsManager assetsManager)
        {
            int fileId = pptr["m_FileID"].AsInt;
            long pathId = pptr["m_PathID"].AsLong;

            AssetsFileInstance targetFile = (fileId == 0)
                ? currentFile
                : assetsManager.LoadAssetsFileFromBundle(bundleFile, fileId, false);  // use fileId as index

            AssetFileInfo targetInfo = targetFile.file.GetAssetInfo(pathId);
            if (targetFile == null)
                throw new Exception("targetFile is null. Failed to resolve fileId.");

            if (targetInfo == null)
                throw new Exception($"Asset with pathId {pathId} not found in file.");

            var baseField = assetsManager.GetBaseField(targetFile, targetInfo);
            if (baseField == null)
                throw new Exception("GetBaseField returned null.");

            return new AssetExternal
            {
                file = targetFile,
                info = targetInfo,
                baseField = assetsManager.GetBaseField(targetFile, targetInfo)
            };
        }

        private static Texture2D ExtractImage(AssetTypeValueField texBase,
                                                AssetsFileInstance assetsFileInst,
                                                BundleFileInstance bunInst,
                                                Rect? cropRect,
                                                ITextureDecoder textureDecoder)
        {
            int width = texBase["m_Width"].AsInt;
            int height = texBase["m_Height"].AsInt;
            int format = texBase["m_TextureFormat"].AsInt;
            byte[] imageBytes = GetImageData(texBase, assetsFileInst, bunInst);

            if (imageBytes == null || imageBytes.Length == 0)
            {
                Debug.LogWarning("imageBytes is null or empty!");
                return null;
            }

            Texture2D LoadImage(byte[] rgbaBytes, TextureFormat textureFormat)
            {
                // Wrap into a Texture2D (RGBA32 ensures consistency)
                Texture2D texture = new(width, height, textureFormat, false);
                texture.LoadRawTextureData(rgbaBytes);
                texture.Apply();

                if (cropRect != null)
                {
                    texture = CropTexture(texture, (Rect)cropRect);
                }

                // // Extract RGBA32
                // Color32[] pixels = texture.GetPixels32();
                // byte[] rgba32 = new byte[pixels.Length * 4];
                // int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Color32));
                // var handle = System.Runtime.InteropServices.GCHandle.Alloc(pixels, System.Runtime.InteropServices.GCHandleType.Pinned);
                // try
                // {
                //     IntPtr ptr = handle.AddrOfPinnedObject();
                //     System.Runtime.InteropServices.Marshal.Copy(ptr, rgba32, 0, rgba32.Length);
                // }
                // finally
                // {
                //     handle.Free();
                // }

                return texture;
            }

            if (IsSupportedFormat((TextureFormat)format, out TextureCompressionFormat compressFormat))
            {
                byte[] bytes = textureDecoder.DecodeToBcBytes(imageBytes, width, height, compressFormat);

                // var decoder = new BcDecoder();
                // ColorRgba32[] decoded = decoder.DecodeRaw(imageBytes, width, height, bcnFormat);
                // byte[] rgbaBytes = new byte[decoded.Length * 4];
                // MemoryMarshal.Cast<ColorRgba32, byte>(decoded.AsSpan()).CopyTo(rgbaBytes);

                return LoadImage(bytes, TextureFormat.RGBA32);
            }
            else if (IsAndroidFormat((TextureFormat)format))
            {
                Texture2D tex = new(width, height, (TextureFormat)format, false);
                tex.LoadRawTextureData(imageBytes);
                tex.Apply();
                Color32[] pixels = tex.GetPixels32();
                // Convert Color32[] to byte[] (RGBA)
                byte[] rgbaBytes = new byte[pixels.Length * 4];
                for (int i = 0; i < pixels.Length; i++)
                {
                    rgbaBytes[i * 4] = pixels[i].r;
                    rgbaBytes[i * 4 + 1] = pixels[i].g;
                    rgbaBytes[i * 4 + 2] = pixels[i].b;
                    rgbaBytes[i * 4 + 3] = pixels[i].a;
                }

                if (rgbaBytes == null || rgbaBytes.Length == 0)
                {
                    Debug.LogWarning("rgbaBytes is null or empty!");
                    return null;
                }

                return LoadImage(rgbaBytes, TextureFormat.RGBA32);
            }
            else if (IsAstcFormat((TextureFormat)format))
            {
                byte[] rgbaBytes = new byte[width * height * 4];
                try
                {
                    (int blockX, int blockY) = GetBlock((TextureFormat)format);
                    if (blockX == -1)
                    {
                        Debug.LogError($"Unsupported ASTC format: {(TextureFormat)format}");
                        return null;
                    }
                    bool result = AstcDecoderNative.DecodeASTC(imageBytes, imageBytes.Length, width, height, rgbaBytes, blockX, blockY);
                    if (result) // assuming 0 means success
                    {
                        Debug.Log("ASTC decode succeeded and texture applied.");
                        return LoadImage(rgbaBytes, TextureFormat.RGBA32);
                    }
                    else
                    {
                        Debug.LogError("ASTC decode failed with error code " + result);
                        return null;
                    }
                }
                catch
                {
                    Debug.LogError("Something went wrong with ASTC decoder dll. Make sure your build work.");
                    return null;
                }
            }
            else if ((TextureFormat)format == TextureFormat.Alpha8)
            {
                int pixels = width * height;
                byte[] rgbaBytes = new byte[pixels * 4];
                for (int i = 0; i < pixels; i++)
                {
                    byte alpha = imageBytes[i];
                    rgbaBytes[i * 4 + 0] = 0; // R
                    rgbaBytes[i * 4 + 1] = 0; // G
                    rgbaBytes[i * 4 + 2] = 0; // B
                    rgbaBytes[i * 4 + 3] = alpha; // A
                }
                return LoadImage(rgbaBytes, TextureFormat.RGBA32);
            }
            else if (imageBytes.Length == width * height * format)
            {
                return LoadImage(imageBytes, (TextureFormat)format);
            }
            else
            {
                Debug.LogError($"Expected {width * height * format} bytes, got {imageBytes.Length}");
                return null;
            }
        }

        private static byte[] GetImageData(AssetTypeValueField texField, AssetsFileInstance fileInst, BundleFileInstance bunInst)
        {
            var imageDataField = texField["image data"];
            byte[] rawData = imageDataField?.Value?.AsByteArray ?? Array.Empty<byte>();

            var streamData = texField["m_StreamData"];
            uint offset = streamData["offset"].AsUInt;
            uint size = streamData["size"].AsUInt;
            string path = streamData["path"].AsString;

            if (!string.IsNullOrEmpty(path) && size > 0)
            {
                if (path.StartsWith("archive:/"))
                {
                    // Extract internal stream file from bundle
                    string internalFileName = Path.GetFileName(path); // e.g. "CAB-c8b157fca857626dbba75589e140a72a.resS"

                    byte[] internalFileData = ExtractFileManually(bunInst, internalFileName);

                    // Read the stream segment from the extracted bytes
                    byte[] buffer = new byte[size];
                    Array.Copy(internalFileData, offset, buffer, 0, size);
                    return buffer;
                }
                else
                {
                    // External file on disk (normal case)
                    string baseDir = Path.GetDirectoryName(fileInst.path);
                    string fullPath = Path.Combine(baseDir, path);

                    if (!File.Exists(fullPath))
                        throw new FileNotFoundException("Stream data file not found", fullPath);

                    byte[] buffer = new byte[size];
                    using (FileStream fs = new(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        fs.Seek(offset, SeekOrigin.Begin);

                        int totalRead = 0;
                        while (totalRead < size)
                        {
                            int read = fs.Read(buffer, totalRead, (int)(size - totalRead));
                            if (read == 0)
                                throw new IOException("Unexpected end of stream while reading external data.");
                            totalRead += read;
                        }
                    }

                    return buffer;
                }
            }

            // No external stream, return rawData
            return rawData;
        }

        private static byte[] ExtractFileManually(BundleFileInstance bundle, string internalFileName)
        {
            var dirInfos = bundle.file.BlockAndDirInfo.DirectoryInfos;

            foreach (var dir in dirInfos)
            {
                if (dir.Name.Equals(internalFileName, StringComparison.OrdinalIgnoreCase))
                {
                    long offset = dir.Offset; // or use dir.OffsetInBundle if that’s the actual name
                    long size = dir.DecompressedSize; // or dir.Size or similar

                    // Make sure the stream is at the beginning of decompressed data
                    var stream = bundle.DataStream;
                    stream.Seek(offset, SeekOrigin.Begin);

                    byte[] buffer = new byte[size];
                    int totalRead = 0;

                    while (totalRead < size)
                    {
                        int read = stream.Read(buffer, totalRead, (int)(size - totalRead));
                        if (read == 0)
                            throw new IOException("Unexpected end of stream while reading internal bundle file.");
                        totalRead += read;
                    }

                    return buffer;
                }
            }

            throw new FileNotFoundException($"File '{internalFileName}' not found in bundle '{bundle.path}'");
        }

        private static bool IsSupportedFormat(TextureFormat unityFormat, out TextureCompressionFormat compressFormat)
        {
            switch (unityFormat)
            {
                // Uncompressed 8-bit/channel formats
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                    compressFormat = TextureCompressionFormat.Rgba;
                    return true;

                case TextureFormat.RGB24:
                    compressFormat = TextureCompressionFormat.Rgb;
                    return true;

                case TextureFormat.R8:
                    compressFormat = TextureCompressionFormat.R;
                    return true;

                case TextureFormat.R16:
                    compressFormat = TextureCompressionFormat.Rg;
                    return true;

                // BCn (DXT) compressed formats
                case TextureFormat.DXT1:
                case TextureFormat.BC4:
                    compressFormat = TextureCompressionFormat.Bc1;
                    return true;

                case TextureFormat.DXT5:
                    compressFormat = TextureCompressionFormat.Bc3;
                    return true;

                case TextureFormat.BC5:
                    compressFormat = TextureCompressionFormat.Bc5;
                    return true;

                case TextureFormat.BC6H:
                    compressFormat = TextureCompressionFormat.Bc6U;
                    return true;

                case TextureFormat.BC7:
                    compressFormat = TextureCompressionFormat.Bc7;
                    return true;

                // Fallback/default
                default:
                    compressFormat = default;
                    return false;
            }
        }

        private static bool IsAndroidFormat(TextureFormat unityFormat)
        {
            return unityFormat == TextureFormat.ETC2_RGB ||
                    unityFormat == TextureFormat.ETC2_RGBA1 ||
                    unityFormat == TextureFormat.ETC2_RGBA8 ||
                    unityFormat == TextureFormat.ETC2_RGBA8Crunched ||
                    unityFormat == TextureFormat.ETC_RGB4 ||
                    unityFormat == TextureFormat.ETC_RGB4Crunched ||
                    unityFormat == TextureFormat.RGBA4444;
        }

        private static bool IsAstcFormat(TextureFormat unityFormat)
        {
            return unityFormat == TextureFormat.ASTC_4x4 ||
                    unityFormat == TextureFormat.ASTC_5x5 ||
                    unityFormat == TextureFormat.ASTC_6x6 ||
                    unityFormat == TextureFormat.ASTC_8x8 ||
                    unityFormat == TextureFormat.ASTC_10x10 ||
                    unityFormat == TextureFormat.ASTC_12x12 ||
                    unityFormat == TextureFormat.ASTC_HDR_4x4 ||
                    unityFormat == TextureFormat.ASTC_HDR_5x5 ||
                    unityFormat == TextureFormat.ASTC_HDR_6x6 ||
                    unityFormat == TextureFormat.ASTC_HDR_8x8 ||
                    unityFormat == TextureFormat.ASTC_10x10 ||
                    unityFormat == TextureFormat.ASTC_12x12;
        }

        private static (int, int) GetBlock(TextureFormat astcFormat)
        {
            return astcFormat switch
            {
                TextureFormat.ASTC_4x4 or TextureFormat.ASTC_HDR_4x4 => (4, 4),
                TextureFormat.ASTC_5x5 or TextureFormat.ASTC_HDR_5x5 => (5, 5),
                TextureFormat.ASTC_6x6 or TextureFormat.ASTC_HDR_6x6 => (6, 6),
                TextureFormat.ASTC_8x8 or TextureFormat.ASTC_HDR_8x8 => (8, 8),
                TextureFormat.ASTC_10x10 or TextureFormat.ASTC_HDR_10x10 => (10, 10),
                TextureFormat.ASTC_12x12 or TextureFormat.ASTC_HDR_12x12 => (12, 12),
                _ => (-1, -1),
            };
        }

        private static Texture2D CropTexture(Texture2D source, Rect rect)
        {
            int x = Mathf.FloorToInt(rect.x);
            int y = Mathf.FloorToInt(rect.y);
            int width = Mathf.FloorToInt(rect.width);
            int height = Mathf.FloorToInt(rect.height);

            // Clamp to source texture bounds
            x = Mathf.Clamp(x, 0, source.width - 1);
            y = Mathf.Clamp(y, 0, source.height - 1);
            width = Mathf.Clamp(width, 1, source.width - x);
            height = Mathf.Clamp(height, 1, source.height - y);

            // Get pixels from the specified rect
            Color[] pixels = source.GetPixels(x, y, width, height);

            // Create new texture and apply pixels
            Texture2D cropped = new(width, height, source.format, false);
            cropped.SetPixels(pixels);
            cropped.Apply();

            return cropped;
        }

        public static string GetAssetTypeValueFieldString(AssetTypeValueField field, int indentLevel = 0)
        {
            if (field == null) return "<null>";

            StringBuilder sb = new();
            string indent = new(' ', indentLevel * 2);

            // Field name and type
            sb.Append(indent);
            sb.Append(field.FieldName);
            sb.Append(" (");
            sb.Append(field.TypeName);
            sb.Append(")");

            // Field value (if any)
            if (field.Value != null)
            {
                sb.Append(" : ");
                sb.Append(field.Value);
            }
            sb.AppendLine();

            // Recursively append children
            if (field.Children != null)
            {
                foreach (var child in field.Children)
                {
                    sb.Append(GetAssetTypeValueFieldString(child, indentLevel + 1));
                }
            }

            return sb.ToString();
        }
    }
}
