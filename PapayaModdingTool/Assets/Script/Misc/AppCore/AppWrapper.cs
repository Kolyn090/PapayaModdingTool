using PapayaModdingTool.Assets.Script.Wrapper.FileBrowser;
using PapayaModdingTool.Assets.Script.Wrapper.Json;
using PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode;
using PapayaModdingTool.Assets.Script.Wrapper.TextureUtil;

namespace PapayaModdingTool.Assets.Script.Misc.AppCore
{
    public class AppWrapper
    {
        public NewtonsoftJsonSerializer JsonSerializer { get; } = new();
        public TextureEncoderDecoder TextureEncoderDecoder { get; } = new();
        public readonly TextureImportExport TextureImportExport;
        public StandaloneFileBrowserWrapper FileBrowser { get; } = new();
        public BcDecoderWrapper BcDecoderWrapper { get; } = new();

        public AppWrapper()
        {
            TextureImportExport = new(TextureEncoderDecoder, TextureEncoderDecoder);
        }
    }
}