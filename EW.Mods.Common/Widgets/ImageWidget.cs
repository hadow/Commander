using System;
using EW.Widgets;
using EW.Graphics;
using EW.Framework.Touch;

namespace EW.Mods.Common.Widgets
{
    public class ImageWidget:Widget
    {
        public string ImageCollection = "";
        public string ImageName = "";
        public bool ClickThrough = true;

        public Func<string> GetImageName;
        public Func<string> GetImageCollection;

        public ImageWidget()
        {
            GetImageName = () => ImageName;
            GetImageCollection = () => ImageCollection;

        }


        public override void Draw()
        {
            var name = GetImageName();
            var collection = GetImageCollection();

            var sprite = ChromeProvider.GetImage(collection, name);

            if (sprite == null)
                throw new ArgumentException("Sprite {0}/{1} was not found".F(collection, name));

            WidgetUtils.DrawRGBA(sprite, RenderOrigin);
        }


        public override bool HandleInput(GestureSample gs)
        {
            return !ClickThrough && RenderBounds.Contains(gs.Position.ToInt2());
        }

    }
}