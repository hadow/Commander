using System;
using EW.Widgets;
using EW.Framework;
namespace EW.Mods.Common.Widgets
{
    public class GridLayout : ILayout
    {
        ScrollPanelWidget widget;
        Int2 pos;

        public GridLayout(ScrollPanelWidget w) { widget = w; }

        public void AdjustChild(Widget w)
        {
            if (widget.Children.Count == 0)
            {
                widget.ContentHeight = 2 * widget.TopBottomSpacing;
                pos = new Int2(widget.ItemSpacing, widget.TopBottomSpacing);
            }

            if (pos.X + w.Bounds.Width + widget.ItemSpacing > widget.Bounds.Width - widget.ScrollbarWidth)
            {
                /* start a new row */
                pos = new Int2(widget.ItemSpacing, widget.ContentHeight - widget.TopBottomSpacing + widget.ItemSpacing);
            }

            w.Bounds.X += pos.X;
            w.Bounds.Y += pos.Y;

            pos = pos.WithX(pos.X + w.Bounds.Width + widget.ItemSpacing);

            widget.ContentHeight = Math.Max(widget.ContentHeight, pos.Y + w.Bounds.Height + widget.TopBottomSpacing);
        }

        public void AdjustChildren() { }
    }
}