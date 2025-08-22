using UnityEngine;

namespace PapayaModdingTool.Assets.Script.DataStruct.TextureData
{
    public class SpriteButtonData
    {
        public Texture2D sprite;
        public string label;
        public int width;
        public int height;
        public Vector2 pivot = Vector2.zero;
        public int level = -1;
        public int order = -1;
        public string animation = "";


        public bool isSelected = false;
    }
}