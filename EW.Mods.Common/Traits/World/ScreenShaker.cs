using System;
using System.Collections.Generic;
using System.Linq;
using EW.Framework;
using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class ScreenShakerInfo : ITraitInfo
    {
        public readonly Vector2 MinMultiplier = new Vector2(-3, -3);
        public readonly Vector2 MaxMultiplier = new Vector2(3, 3);


        public object Create(ActorInitializer init) { return new ScreenShaker(this); }
    }

    public class ScreenShaker:ITick,IWorldLoaded
    {

        struct ShakeEffect
        {
            public int ExpiryTime;
            public WPos Position;
            public int Intensity;
            public Vector2 Multiplier;
        }

        readonly ScreenShakerInfo info;
        WorldRenderer worldRenderer;
        List<ShakeEffect> shakeEffects = new List<ShakeEffect>();

        int ticks = 0;
        public ScreenShaker(ScreenShakerInfo info)
        {
            this.info = info;
        }


        void IWorldLoaded.WorldLoaded(World w,WorldRenderer wr)
        {
            worldRenderer = wr;
        }


        void ITick.Tick(Actor self)
        {
            if (shakeEffects.Any())
            {
                worldRenderer.ViewPort.Scroll(GetScrollOffset(), true);
                shakeEffects.RemoveAll(t => t.ExpiryTime == ticks);
            }

            ticks++;
        }

        Vector2 GetScrollOffset()
        {
            return GetMultiplier() * GetIntensity() * new Vector2((float)Math.Sin((ticks * 2 * Math.PI) / 4), (float)Math.Cos((ticks * 2 * Math.PI) / 5));
        }

        Vector2 GetMultiplier()
        {
            return shakeEffects.Aggregate(Vector2.Zero, (sum, next) => sum + next.Multiplier).Constrain(info.MinMultiplier, info.MaxMultiplier);
        }

        float GetIntensity()
        {
            var cp = worldRenderer.ViewPort.CenterPosition;
            var intensity = 100 * 1024 * 1024 * shakeEffects.Sum(e => (float)e.Intensity / (e.Position - cp).LengthSquared);

            return Math.Min(intensity, 10);
        }


        public void AddEffect(int time, WPos position, int intensity)
        {
            AddEffect(time, position, intensity, new Vector2(1, 1));
        }

        public void AddEffect(int time, WPos position, int intensity, Vector2 multiplier)
        {
            shakeEffects.Add(new ShakeEffect { ExpiryTime = ticks + time, Position = position, Intensity = intensity, Multiplier = multiplier });
        }


    }
}