using System;
using System.Drawing;
using EW.Widgets;
using EW.Framework;
using EW.Framework.Touch;

namespace EW.Mods.Common.Widgets
{
    public class BackgroundWidget:Widget
    {
        public readonly string Background = "dialog";
        public readonly bool ClickThrough = false;
        public readonly bool Draggable = false;

        bool moving;
        Int2? prevMouseLocation;

        public BackgroundWidget() { }

        public override void Draw()
        {
            WidgetUtils.DrawPanel(Background, RenderBounds);
        }

        public override bool HandleInput(GestureSample gs)
        {
            if (ClickThrough || !RenderBounds.Contains(gs.Position.ToInt2()))
                return false;


            if (!Draggable || (moving && (!TakeFocus(gs) || gs.GestureType == GestureType.Tap)))
                return true;

            if (prevMouseLocation != null)
                prevMouseLocation = gs.Position.ToInt2();

            var vec = gs.Position.ToInt2() - (Int2)prevMouseLocation;
            prevMouseLocation = gs.Position.ToInt2();

            switch (gs.GestureType)
            {
                case GestureType.DragComplete:
                    moving = false;
                    YieldFocus(gs);
                    break;
                case GestureType.Tap:
                    moving = true;
                    Bounds = new Rectangle(Bounds.X + vec.X, Bounds.Y + vec.Y, Bounds.Width, Bounds.Height);
                    break;
                case GestureType.FreeDrag:
                    //if (moving)
                        Bounds = new Rectangle(Bounds.X+vec.X, Bounds.Y + vec.Y, Bounds.Width, Bounds.Height);
                    break;
            }
            return true;
        }



    }
}