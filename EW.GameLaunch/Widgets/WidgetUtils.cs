using System;
using System.Drawing;
using System.Linq;
using EW.Graphics;
using EW.Framework;
namespace EW.Widgets
{
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

    }
}