using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class MapCreepsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new MapCreeps(this); }
    }


    public class MapCreeps:INotifyCreated
    {
        readonly MapCreepsInfo info;
        public bool Enabled { get; private set; }

        public MapCreeps(MapCreepsInfo info)
        {
            this.info = info;
        }

        void INotifyCreated.Created(Actor self)
        {
            Enabled = true;
        }
    }
}