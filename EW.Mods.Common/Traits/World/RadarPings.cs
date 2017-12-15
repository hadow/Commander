using System;
using System.Collections.Generic;
using EW.Traits;
using EW.OpenGLES;
namespace EW.Mods.Common.Traits
{
    public class RadarPingsInfo : ITraitInfo
    {
        public readonly int FromRadius = 200;
        public readonly int ToRadius = 15;
        public readonly int ShrinkSpeed = 4;
        public readonly float RotationSpeed = 0.12f;

        public object Create(ActorInitializer init)
        {
            return new RadarPings(this);
        }
    }
    public class RadarPings:ITick
    {
        public readonly List<RadarPing> Pings = new List<RadarPing>();
        readonly RadarPingsInfo info;

        public RadarPings(RadarPingsInfo info)
        {
            this.info = info;
        }
        public void Tick(Actor self)
        {
            foreach (var ping in Pings.ToArray())
                if (!ping.Tick())
                    Pings.Remove(ping);
        }

    }

    public class RadarPing
    {
        public Func<bool> IsVisible;
        public WPos Position;
        public Color Color;
        public int Duration;
        public int FromRadius;
        public int ToRadius;
        public int ShrinkSpeed;//缩放速度
        public float RotationSpeed;

        int radius;
        float angle;
        int tick;

        public RadarPing(Func<bool> isVisible,WPos position,Color color,
            int duration,int fromRadius,int toRadius,int shrinkSpeed,float rotationSpeed)
        {
            IsVisible = isVisible;
            Position = position;
            Color = color;
            Duration = duration;
            FromRadius = fromRadius;
            ToRadius = toRadius;
            ShrinkSpeed = shrinkSpeed;
            RotationSpeed = rotationSpeed;

            radius = fromRadius;
        }

        public bool Tick()
        {
            if (++tick == Duration)
                return false;

            return true;
        }
    }
}