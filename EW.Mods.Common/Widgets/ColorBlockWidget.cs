using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Widgets;

namespace EW.Mods.Common.Widgets
{
    public class ColorBlockWidget:Widget
    {
        public Func<Color> GetColor;

        public ColorBlockWidget()
        {
            GetColor = () => Color.White;
        }


        public override void Draw()
        {
            WidgetUtils.FillRectWithColor(RenderBounds, GetColor());
        }
    }
}