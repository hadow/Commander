using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class GateInfo:BuildingInfo
    {

    }

    public class Gate : Building
    {
        public Gate(ActorInitializer init,GateInfo info):base(init,info)
        {

        }
    }
}