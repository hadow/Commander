using System;
using System.Drawing;
using System.Linq;
using EW.Support;
using EW.Widgets;
using EW.Framework;

namespace EW.Mods.Common.Widgets
{
    public class PerfGraphWidget:Widget
    {

        public override void Draw()
        {
            var cr = WarGame.Renderer.RgbaColorRenderer;
            var rect = RenderBounds;
            var origin = new Vector2(rect.Right, rect.Bottom);
            var basis = new Vector2(-rect.Width / 100, -rect.Height / 100);

            cr.DrawLine(new[]
            {
                new Vector3(rect.Left,rect.Top,0),
                new Vector3(rect.Left,rect.Bottom,0),
                new Vector3(rect.Right,rect.Bottom,0)
            }, 1, Color.White);

            cr.DrawLine(origin + new Vector2(100, 0) * basis, origin + new Vector2(100, 100) * basis, 1, Color.White);

            var k = 0;
            foreach(var item in PerfHistory.Items.Values)
            {
                cr.DrawLine(item.Samples().Select((sample, i) => origin + new Vector3(i, (float)sample, 0) * basis), 1, item.C);

                var u = new Vector2(rect.Left, rect.Top);

                cr.DrawLine(
                    u + new Vector2(10, 10 * k + 5),
                    u + new Vector2(12, 10 * k + 5), 1, item.C);

                cr.DrawLine(
                    u + new Vector2(10, 10 * k + 4),
                    u + new Vector2(12, 10 * k + 4), 1, item.C);

                ++k;
            }

            k = 0;
            foreach(var item in PerfHistory.Items.Values)
            {
                WarGame.Renderer.Fonts["Tiny"].DrawText(item.Name, new Vector2(rect.Left, rect.Top) + new Vector2(18, 10 * k - 3), Color.White);
                ++k;
            }

        }


    }
}