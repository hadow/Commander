using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Modifies the terrain type underneath the actors location.
    /// </summary>
    class ChangesTerrainInfo:ITraitInfo
    {
        [FieldLoader.Require]
        public readonly string TerrainType = null;

        public object Create(ActorInitializer init) { return new ChangesTerrain(this); }
    }

    class ChangesTerrain:INotifyAddedToWorld,INotifyRemovedFromWorld
    {

        readonly ChangesTerrainInfo info;

        byte previousTerrain;

        public ChangesTerrain(ChangesTerrainInfo info)
        {
            this.info = info;
        }


        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            var cell = self.Location;
            var map = self.World.Map;
            var terrain = map.Rules.TileSet.GetTerrainIndex(info.TerrainType);
            previousTerrain = map.CustomTerrain[cell];
            map.CustomTerrain[cell] = terrain;
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            var cell = self.Location;
            var map = self.World.Map;
            map.CustomTerrain[cell] = previousTerrain;

        }

    }
}