using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class JamsMissilesInfo:ITraitInfo
    {
        public readonly WDist Range = WDist.Zero;

        public readonly Stance DeflectionStances = Stance.Ally | Stance.Neutral | Stance.Enemy;

        public readonly int Chance = 100;

        public object Create(ActorInitializer init) { return new JamsMissiles(this); }
    }


    public class JamsMissiles
    {
        readonly JamsMissilesInfo info;

        public WDist Range { get { return info.Range; } }

        public Stance DeflectionStances { get { return info.DeflectionStances; } }

        public int Chance { get { return info.Chance; } }

        public JamsMissiles(JamsMissilesInfo info) { this.info = info; }



    }
}