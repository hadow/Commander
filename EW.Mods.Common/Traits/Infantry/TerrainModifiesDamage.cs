using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Warheads;
namespace EW.Mods.Common.Traits
{
    public class TerrainModifiesDamageInfo : ITraitInfo
    {
        [FieldLoader.Require]
        public readonly Dictionary<string, int> TerrainModifier = null;

        public readonly bool ModifyHealing = false;

        public object Create(ActorInitializer init) { return new TerrainModifiesDamage(init.Self,this); }
    }

    public class TerrainModifiesDamage:IDamageModifier
    {
        const int FullDamage = 100;

        public readonly TerrainModifiesDamageInfo Info;

        readonly Actor self;

        public TerrainModifiesDamage(Actor self,TerrainModifiesDamageInfo info)
        {
            Info = info;
            this.self = self;
        }

        public int GetDamageModifier(Actor attacker,IWarHead warhead)
        {
            var damageWh = warhead as DamageWarhead;

            var world = self.World;
            var map = world.Map;

            var pos = map.CellContaining(self.CenterPosition);

            var terrainType = map.GetTerrainInfo(pos).Type;

            if (!Info.TerrainModifier.ContainsKey(terrainType))
                return FullDamage;
            return Info.TerrainModifier[terrainType];
        }
    }
}