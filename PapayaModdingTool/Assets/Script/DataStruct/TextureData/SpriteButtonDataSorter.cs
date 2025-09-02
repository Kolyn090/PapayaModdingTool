using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PapayaModdingTool.Assets.Script.DataStruct.TextureData
{
    public static class SpriteButtonDataSorter
    {
        public static List<SpriteButtonData> SortByLabel(List<SpriteButtonData> datas)
        {
            return datas
                .OrderBy(o =>
                {
                    // prefix = everything before the trailing digits
                    var prefix = Regex.Replace(o.label, @"\d+$", "");
                    return prefix;
                })
                .ThenBy(o =>
                {
                    // numeric suffix (or max if none)
                    var match = Regex.Match(o.label, @"\d+$");
                    return match.Success && int.TryParse(match.Value, out int num)
                        ? num
                        : int.MaxValue;
                })
                .ToList();
        }

        public static List<SpriteButtonData> SortByWorkplace(List<SpriteButtonData> datas)
        {
            return datas.OrderBy(o => o.level).ThenBy(o => o.order).ToList();
        }
    }
}