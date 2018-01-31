using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Effects;
using EW.Graphics;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Effects
{
    public class FloatingText:IEffect,IEffectAboveShroud
    {
        static readonly WVec Velocity = new WVec(0, 0, 86);

        readonly SpriteFont font;

        readonly string text;

        Color color;
        int remaining;
        WPos pos;

        public FloatingText(WPos pos,Color color,string text,int duration)
        {
            font = WarGame.Renderer.Fonts["TinyBold"];

            this.pos = pos;
            this.color = color;
            this.text = text;
            remaining = duration;
        }

        void IEffect.Tick(World world)
        {

            if (--remaining <= 0)
                world.AddFrameEndTask(w => w.Remove(this));

            pos +=Velocity;
        }

        IEnumerable<IRenderable> IEffect.Render(WorldRenderer wr) { return SpriteRenderable.None; }

        IEnumerable<IRenderable> IEffectAboveShroud.RenderAboveShroud(WorldRenderer wr)
        {
            if (wr.World.FogObscures(pos) || wr.World.ShroudObscures(pos))
                yield break;

            //Arbitrary large value used for the z-offset to try and ensure the text displays above everything else.
            yield return new TextRenderable(font, pos, 4096, color, text);
        }


        public static string FormatCashTick(int cashAmount)
        {
            return "{0}${1}".F(cashAmount < 0 ? "-" : "+", Math.Abs(cashAmount));
        }
    }
}