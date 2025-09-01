using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PapayaModdingTool.Assets.Script.DataStruct.TextureData
{
    public static class SpriteButtonDataSorter
    {
        public static List<SpriteButtonData> SortByOriginalLabel(List<SpriteButtonData> datas)
        {
            return datas.OrderBy(o => o.originalLabel)
            .ThenBy(o =>
            {
                var match = Regex.Match(o.originalLabel, @"\d+$");
                if (match.Success && int.TryParse(match.Value, out int num))
                    return num;
                else
                    return int.MaxValue; // no number → push to end
            })
            .ToList();
        }

        public static List<SpriteButtonData> SortByWorkplace(List<SpriteButtonData> datas)
        {
            return datas.OrderBy(o => o.level).ThenBy(o => o.order).ToList();
        }
    }
}