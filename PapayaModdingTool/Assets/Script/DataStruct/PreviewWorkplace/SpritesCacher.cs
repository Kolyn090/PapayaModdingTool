using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;

namespace PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace
{
    /// <summary>
    /// Cache sprites in sprites panel so that we don't have to completely reload
    /// each time we open texture.
    /// </summary>
    public class SpritesCacher
    {
        private readonly Dictionary<Texture2DButtonData, List<SpriteButtonData>> _cache = new();

        public void AddToCache(Texture2DButtonData texture2dData, List<SpriteButtonData> spriteDatas)
        {
            _cache[texture2dData] = spriteDatas;
        }

        public void RemoveFromCache(Texture2DButtonData texture2dData)
        {
            if (_cache.ContainsKey(texture2dData))
            {
                _cache.Remove(texture2dData);
            }
        }

        public List<SpriteButtonData> GetFromCache(Texture2DButtonData texture2dData)
        {
            if (_cache.ContainsKey(texture2dData))
            {
                return _cache[texture2dData];
            }
            return null;
        }
    }
}