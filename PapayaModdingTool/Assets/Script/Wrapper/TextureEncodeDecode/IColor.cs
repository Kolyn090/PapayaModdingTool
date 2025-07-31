using System;

namespace PapayaModdingTool.Assets.Script.Wrapper.TextureEncodeDecode
{
    public interface IColor : IEquatable<IColor>
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }
        public byte A { get; }
    }
}