using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;

namespace EW.Mods.Common.Traits
{

    class DamagedByTerrainInfo : ConditionalTraitInfo,Requires<HealthInfo>
    {

        [FieldLoader.Require]
        public readonly int Damage = 0;

        public readonly int DamageInterval = 0;

        public readonly HashSet<string> DamageTypes = new HashSet<string>();

        [FieldLoader.Require]
        public readonly string[] Terrain = { };

        //低于阀值不会进一步受到伤害
        [Desc("Percentage health below which the actor will not receive further damage.")]
        public readonly int DamageThreshold = 0;

        public readonly bool StartOnThreshold = false;




        public override object Create(ActorInitializer init)
        {
            return new DamagedByTerrain(init.Self, this);
        }
    }
    class DamagedByTerrain:ConditionalTrait<DamagedByTerrainInfo>,ITick,ISync,INotifyAddedToWorld
    {
        readonly Health health;

        [Sync]int damageTicks;
        [Sync]int damageThreshold;





        public DamagedByTerrain(Actor self,DamagedByTerrainInfo info) : base(info) {

            health = self.Trait<Health>();
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self){

            if (!Info.StartOnThreshold)
                return;

            var safeTiles = 0;
            var totalTiles = 0;

            foreach(var kv in self.OccupiesSpace.OccupiedCells())
            {
                totalTiles++;

                if (!Info.Terrain.Contains(self.World.Map.GetTerrainInfo(kv.First).Type))
                    safeTiles++;
                
            }
            if (totalTiles == 0)
                return;

            damageThreshold = (Info.DamageThreshold * health.MaxHP + (100 - Info.DamageThreshold) * safeTiles * health.MaxHP / totalTiles) / 100;

            var delta = health.HP - damageThreshold;
            if (delta > 0)
                self.InflictDamage(self.World.WorldActor, new Damage(delta, Info.DamageTypes));

        }

        void ITick.Tick(Actor self){

            if (IsTraitDisabled || health.HP <= damageThreshold || --damageTicks > 0)
                return;

            if (!self.IsInWorld)
                return;

            var t = self.World.Map.GetTerrainInfo(self.Location);
            if (!Info.Terrain.Contains(t.Type))
                return;

            self.InflictDamage(self.World.WorldActor,new Damage(Info.Damage,Info.DamageTypes));

            damageTicks = Info.DamageInterval;

        }
    }
}