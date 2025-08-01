using System.Collections.Generic;
using System.IO;
using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.DataStruct.FileRead;
using PapayaModdingTool.Assets.Script.Wrapper.Json;

namespace PapayaModdingTool.Assets.Script.Reader
{
    public class DumpReader
    {
        private readonly AssetsManager _assetsManager;

        public DumpReader(AssetsManager am)
        {
            _assetsManager = am;
        }

        public List<DumpInfo> ReadSpriteAtlasDumps(AssetsFileInstance fileInst)
        {
            return ReadDumps(fileInst, AssetClassID.SpriteAtlas);
        }

        public List<DumpInfo> ReadSpriteDumps(AssetsFileInstance fileInst)
        {
            return ReadDumps(fileInst, AssetClassID.Sprite);
        }

        public List<DumpInfo> ReadSpriteAtlasDumps(BundleFileInstance bunInst)
        {
            return ReadDumps(bunInst, AssetClassID.SpriteAtlas);
        }

        public List<DumpInfo> ReadSpriteDumps(BundleFileInstance bunInst)
        {
            return ReadDumps(bunInst, AssetClassID.Sprite);
        }

        public List<DumpInfo> ReadTexture2DDumps(BundleFileInstance bunInst)
        {
            return ReadDumps(bunInst, AssetClassID.Texture2D);
        }

        private List<DumpInfo> ReadDumps(BundleFileInstance bunInst, AssetClassID assetType)
        {
            return ReadDumps(_assetsManager.LoadAssetsFileFromBundle(bunInst, 0, false), assetType);
        }

        private List<DumpInfo> ReadDumps(AssetsFileInstance fileInst, AssetClassID assetType)
        {
            List<DumpInfo> result = new();

            List<AssetFileInfo> assetInfos = fileInst.file.GetAssetsOfType(assetType);
            if (assetInfos.Count == 0)
                return result;

            foreach (var assetInfo in assetInfos)
            {
                AssetTypeValueField assetBase = _assetsManager.GetBaseField(fileInst, assetInfo);
                result.Add(new()
                {
                    dumpJson = new NewtonsoftJsonObject(JsonDumper.RecurseJsonDump(assetBase, true)),
                    pathID = assetInfo.PathId
                });
            }

            return result;
        }
        
        public static string ReadCABCode(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            byte[] buffer = new byte[fs.Length];
            br.Read(buffer, 0, buffer.Length);

            byte[] cabPrefix = Encoding.ASCII.GetBytes("CAB-");

            for (int i = 0; i <= buffer.Length - cabPrefix.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < cabPrefix.Length; j++)
                {
                    if (buffer[i + j] != cabPrefix[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    int start = i;
                    int end = start;

                    // Read until whitespace or non-printable ASCII
                    while (end < buffer.Length)
                    {
                        byte b = buffer[end];
                        if (b < 0x21 || b > 0x7E) // non-printable ASCII
                            break;
                        end++;
                    }

                    string cab = Encoding.ASCII.GetString(buffer, start, end - start - 1);
                    return cab;
                }
            }

            return "";
        }
    }
}