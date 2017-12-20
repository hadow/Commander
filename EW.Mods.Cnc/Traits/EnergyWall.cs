using System;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{
    public class EnergyWallInfo:BuildingInfo,IObservesVariablesInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new EnergyWall(init, this);
        }
    }


    public class EnergyWall:Building
    {

        public EnergyWall(ActorInitializer init,EnergyWallInfo info) : base(init, info)
        {

        }
    }
}