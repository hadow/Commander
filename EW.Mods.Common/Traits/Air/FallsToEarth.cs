using System;
using System.Collections.Generic;

using EW.Traits;
using EW.Mods.Common.Activities;

namespace EW.Mods.Common.Traits
{
    public class FallsToEarthInfo : ITraitInfo,IRulesetLoaded,Requires<AircraftInfo>
    {


        public readonly string Explosion = "UnitExplode";

        public readonly bool Spins = true;
        public readonly bool Moves = false;

        public readonly WDist Velocity = new WDist(43);

        public WeaponInfo ExplosionWeapon { get; private set; }



        public void RulesetLoaded(Ruleset rules,ActorInfo ai){

            if (string.IsNullOrEmpty(Explosion))
                return;

            WeaponInfo weapon;
            var weaponToLower = Explosion.ToLowerInvariant();
            if (!rules.Weapons.TryGetValue(weaponToLower, out weapon))
                throw new YamlException("Weapons ruleset does not contain an entry '{0}'".F(weaponToLower));

            ExplosionWeapon = weapon;

        }
        public object Create(ActorInitializer init)
        {
            return new FallsToEarth(init.Self,this);
        }
    }

    public class FallsToEarth
    {

        public FallsToEarth(Actor self,FallsToEarthInfo info){
            self.QueueActivity(false,new FallToEarth(self,info));
        }
    }
}