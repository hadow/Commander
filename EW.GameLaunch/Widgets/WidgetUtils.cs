using System;
using System.Drawing;
using System.Linq;
using EW.Graphics;
using EW.Framework;
namespace EW.Widgets
{

    [Flags]
    public enum PanelSides
    {
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        Center = 16,

        Edges = Left | Top | Right| Bottom,
        All = Edges | Center,
    }
    public static class WidgetUtils
    {

        public static string FormatTime(int ticks,int timestep)
        {
            return FormatTime(ticks, true, timestep);
        }
        public static string FormatTime(int ticks,bool leadingMinuteZero,int timestep)
        {
            var seconds = (int)Math.Ceiling(ticks * timestep / 1000f);
            return FormatTimeSeconds(seconds, leadingMinuteZero);
        }

        public static string FormatTimeSeconds(int seconds)
        {
            return FormatTimeSeconds(seconds, true);
        }

        public static string FormatTimeSeconds(int seconds,bool leadingMinuteZero)
        {
            var minutes = seconds / 60;

            if (minutes >= 60)
                return "{0:D}:{1:D2}:{2:D2}".F(minutes / 60, minutes % 60, seconds % 60);
            if (leadingMinuteZero)
                return "{0:D2}:{1:D2}".F(minutes, seconds % 60);
            return "{0:D}:{1:D2}".F(minutes, seconds % 60);
        }


        public static void FillRectWithColor(Rectangle r,Color c)
        {
            WarGame.Renderer.RgbaColorRenderer.FillRect(new Vector2(r.Left, r.Top), new Vector2(r.Right, r.Bottom), c);
        }

        public static void FillRectWithSprite(Rectangle r,Sprite s)
        {
            for(var x=r.Left;x<r.Right;x+=(int)s.Size.X)
                for(var y = r.Top;y<r.Bottom;y+=(int)s.Size.Y)
                {
                    var ss = s;
                    var left = new Int2(r.Right - x, r.Bottom - y);
                    if(left.X<(int)s.Size.X || left.Y < (int)s.Size.Y)
                    {
                        var rr = new Rectangle(s.Bounds.Left,
                            s.Bounds.Top,
                            Math.Min(left.X, (int)s.Size.X),
                            Math.Min(left.Y, (int)s.Size.Y));
                        ss = new Sprite(s.Sheet, rr, s.Channel);

                        DrawRGBA(ss, new Vector2(x, y));
                    }

                }
        }

        public static void DrawRGBA(Sprite s,Vector2 pos)
        {
            WarGame.Renderer.RgbaSpriteRenderer.DrawSprite(s, pos);
        }

        public static void DrawPanel(string collection,Rectangle bounds)
        {
            DrawPanelPartial(collection, bounds, PanelSides.All);
        }

        public static void DrawPanelPartial(string collection,Rectangle bounds,PanelSides ps)
        {
            DrawPanelPartial(bounds, ps,
                ChromeProvider.GetImage(collection, "border-t"),
                ChromeProvider.GetImage(collection, "border-b"),
                ChromeProvider.GetImage(collection,"border-l"),
                ChromeProvider.GetImage(collection,"border-r"),
                ChromeProvider.GetImage(collection,"corner-tl"),
                ChromeProvider.GetImage(collection,"corner-tr"),
                ChromeProvider.GetImage(collection,"corner-bl"),
                ChromeProvider.GetImage(collection,"corner-br"),
                ChromeProvider.GetImage(collection,"background"));
        }

        public static void DrawPanelPartial(Rectangle bounds,PanelSides ps,
            Sprite borderTop,
            Sprite borderBottom,
            Sprite borderLeft,
            Sprite borderRight,
            Sprite cornerTopLeft,
            Sprite cornerTopRight,
            Sprite cornerBottomLeft,
            Sprite cornerBottomRight,
            Sprite background)
        {

            var marginLeft = borderLeft == null ? 0 : (int)borderLeft.Size.X;
            var marginTop = borderTop == null ? 0 : (int)borderTop.Size.Y;
            var marginRight = borderRight == null ? 0 : (int)borderRight.Size.X;
            var marginBottom = borderBottom == null ? 0 : (int)borderBottom.Size.Y;
            var marginWidth = marginRight + marginLeft;
            var marginHeight = marginBottom + marginTop;

            //Background
            if (ps.HasFlags(PanelSides.Center) && background != null)
                FillRectWithSprite(new Rectangle(bounds.Left + marginLeft, bounds.Top + marginTop, bounds.Width - marginWidth, bounds.Height - marginHeight), background);

            if (ps.HasFlags(PanelSides.Left) && borderLeft != null)
                FillRectWithSprite(new Rectangle(bounds.Left, bounds.Top + marginTop, marginLeft, bounds.Height - marginHeight), borderLeft);

            if (ps.HasFlags(PanelSides.Right) && borderRight != null)
                FillRectWithSprite(new Rectangle(bounds.Right - marginRight, bounds.Top + marginTop, marginLeft, bounds.Height - marginHeight), borderRight);

            if (ps.HasFlags(PanelSides.Top) && borderTop != null)
                FillRectWithSprite(new Rectangle(bounds.Left + marginLeft, bounds.Top, bounds.Width - marginWidth, marginTop), borderTop);

            if (ps.HasFlags(PanelSides.Bottom) && borderBottom != null)
                FillRectWithSprite(new Rectangle(bounds.Left + marginLeft, bounds.Bottom - marginBottom, bounds.Width - marginWidth, marginTop), borderBottom);

            if (ps.HasFlags(PanelSides.Left | PanelSides.Top) && cornerTopLeft != null)
                DrawRGBA(cornerTopLeft, new Vector2(bounds.Left, bounds.Top));

            if (ps.HasFlags(PanelSides.Right | PanelSides.Top) && cornerTopRight != null)
                DrawRGBA(cornerTopRight, new Vector2(bounds.Right - cornerTopRight.Size.X, bounds.Top));

            if (ps.HasFlags(PanelSides.Left | PanelSides.Bottom) && cornerBottomLeft != null)
                DrawRGBA(cornerBottomLeft, new Vector2(bounds.Left, bounds.Bottom - cornerBottomLeft.Size.Y));

            if (ps.HasFlags(PanelSides.Right | PanelSides.Bottom) && cornerBottomRight != null)
                DrawRGBA(cornerBottomRight, new Vector2(bounds.Right - cornerBottomRight.Size.X, bounds.Bottom - cornerBottomRight.Size.Y));

        }

        static bool HasFlags(this PanelSides a,PanelSides b)
        {
            return (a & b) == b;
        }

        public static Color GetContrastColor(Color fgColor,Color bgDark,Color bgLight)
        {
            var fg = new HSLColor(fgColor);
            return fg.RGB == Color.White || fg.L > 80 ? bgDark : bgLight;
        }

    }
}