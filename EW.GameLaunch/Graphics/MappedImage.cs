using System;
using System.Collections.Generic;
using System.Drawing;
namespace EW.Graphics
{
    class MappedImage
    {
        readonly Rectangle rect = Rectangle.Empty;
        public readonly string Src;

        public MappedImage(string defaultSrc,MiniYaml info)
        {
            FieldLoader.LoadField(this, "rect", info.Value);
            FieldLoader.Load(this, info);
            if (Src == null)
                Src = defaultSrc;

        }

        public Sprite GetImage(Sheet s)
        {
            return new Sprite(s, rect, TextureChannel.Alpha);
        }
    }
}