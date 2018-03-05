using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class BridgeLayerInfo : ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new BridgeLayer(init.World);
        }
    }


    class BridgeLayer
    {

        readonly CellLayer<Actor> bridges;

        public BridgeLayer(World world)
        {
            bridges = new CellLayer<Actor>(world.Map);
        }

        public Actor this[CPos cell] { get { return bridges[cell]; } }

        public void Add(Actor b){

            var buildingInfo = b.Info.TraitInfo<BuildingInfo>();
            foreach(var c in buildingInfo.PathableTiles(b.Location)){

                bridges[c] = b;
            }
        }


        public void Remove(Actor b){

            var buildingInfo = b.Info.TraitInfo<BuildingInfo>();
            foreach(var c in buildingInfo.PathableTiles(b.Location)){
                bridges[c] = null;
            }
        }
    }
}