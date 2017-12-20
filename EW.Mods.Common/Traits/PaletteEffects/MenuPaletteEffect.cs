using System;

using EW.Traits;
using EW.Graphics;
using EW.OpenGLES;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Fades the world from/to black at the start/end of the gam,and can(optionally) desaturate the world.
    /// </summary>
    public class MenuPaletteEffectInfo : ITraitInfo
    {
        public readonly int FadeLength = 10;

        public readonly MenuPaletteEffect.EffectType Effect = MenuPaletteEffect.EffectType.None;

        public readonly MenuPaletteEffect.EffectType MenuEffect = MenuPaletteEffect.EffectType.None;

        

        public object Create(ActorInitializer init)
        {
            return new MenuPaletteEffect(this);
        }
    }
    public class MenuPaletteEffect:IPaletteModifier,ITickRender,IWorldLoaded
    {
        public enum EffectType { None,Black,Desaturated}

        public readonly MenuPaletteEffectInfo Info;

        int remainingFrames;

        EffectType from = EffectType.Black;
        EffectType to = EffectType.Black;

        public MenuPaletteEffect(MenuPaletteEffectInfo info) { this.Info = info; }



        public void WorldLoaded(World world, WorldRenderer wr)
        {
            Fade(Info.Effect);
        }


        public void Fade(EffectType type)
        {
            remainingFrames = Info.FadeLength;
            from = to;
            to = type;
        }

        public void AdjustPalette(IReadOnlyDictionary<string,MutablePalette> palettes)
        {
            if (to == EffectType.None && remainingFrames == 0)
                return;

            foreach(var pal in palettes.Values)
            {
                for(var x = 0; x < Palette.Size; x++)
                {
                    var orig = pal.GetColor(x);
                    var t = ColorForEffect(to, orig);

                    if (remainingFrames == 0)
                        pal.SetColor(x, t);
                    else
                    {
                        var f = ColorForEffect(from, orig);
                        pal.SetColor(x, Exts.ColorLerp((float)remainingFrames / Info.FadeLength, t, f));
                    }
                }
            }
        }

        static Color ColorForEffect(EffectType t, Color orig)
        {
            switch (t)
            {
                case EffectType.Black:
                    return Color.FromArgb(orig.A,Color.Black);
                case EffectType.Desaturated:
                    var lum = (int)(255 * orig.GetBrightness());
                    return Color.FromArgb(orig.A,lum,lum,lum);
                default:
                case EffectType.None:
                    return orig;
            }
        }

        public void TickRender(WorldRenderer wr,Actor self)
        {
            if (remainingFrames > 0)
                remainingFrames--;
        }

    }
}