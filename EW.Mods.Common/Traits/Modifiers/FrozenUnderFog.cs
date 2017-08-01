using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class FrozenUnderFogInfo : ITraitInfo, Requires<BuildingInfo>
    {

        public object Create(ActorInitializer init) { return new FrozenUnderFog(init,this); }
    }

    public class FrozenUnderFog:IDefaultVisibility,ITick,ISync,INotifyCreated
    {



        public FrozenUnderFog(ActorInitializer init,FrozenUnderFogInfo info)
        {

        }

        public void Created(Actor self) { }

        public bool IsVisible(Actor self,Player byPlayer)
        {
            return true;
        }

        public void Tick(Actor self)
        {

        }

    }
}