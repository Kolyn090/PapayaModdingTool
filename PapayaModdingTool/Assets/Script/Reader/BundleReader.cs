using System;
using System.IO;
using AssetsTools.NET.Extra;
using PapayaModdingTool.Assets.Script.Dispatcher;

namespace PapayaModdingTool.Assets.Script.Reader
{
    public class BundleReader
    {
        private readonly AssetsManager _assetsManager;
        private readonly EventDispatcher _dispatcher;
        private readonly AssetsReader _assetsReader;

        public BundleReader(AssetsManager assetsManager, EventDispatcher dispatcher)
        {
            _assetsManager = assetsManager;
            _dispatcher = dispatcher;
            _assetsReader = new AssetsReader(_assetsManager);
        }

        public (BundleFileInstance, AssetsFileInstance) ReadBundle(string path)
        {
            if (Path.GetExtension(path).Equals(".assets", StringComparison.OrdinalIgnoreCase))
            {
                // Load as standalone .assets file
                // var assetsInst = _assetsManager.LoadAssetsFile(path, true);
                var assetsInst = _assetsReader.ReadValidAssetsFileInst(path, true);
                // _dispatcher.Dispatch(new BundleReadEvent(null, path, assetsInst));

                return (null, assetsInst);
            }
            else
            {
                // Load as bundle
                var assetsInst = _assetsReader.ReadValidAssetsFileInst(path, false);
                var bunInst = _assetsManager.LoadBundleFile(path, true);
                // _dispatcher.Dispatch(new BundleReadEvent(bunInst, path, assetsInst));
                return (bunInst, assetsInst);
            }
        }
    }
}