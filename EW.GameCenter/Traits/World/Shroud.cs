using System;
using System.Collections.Generic;

namespace EW.Traits
{
    public class ShroudInfo:ITraitInfo
    {
        public bool FogEnabled = true;

        public bool FogLocked = false;

        public bool ExploredMapEnabled = false;

        public bool ExploredMapLocked = false;

        public object Create(ActorInitializer init) { return new Shroud(init.Self,this); }
    }


    public class Shroud:ISync,INotifyCreated
    {
        readonly Actor self;
        readonly ShroudInfo info;
        readonly Map map;

        readonly CellLayer<short> visibleCount;
        readonly CellLayer<short> generatedShroudCount;
        readonly CellLayer<bool> explored;


        [Sync]
        bool disabled;

        public bool Disabled
        {
            get { return disabled; }
            set
            {
                if (disabled == value)
                    return;
                disabled = value;

            }
        }

        bool fogEnabled;

        public bool FogEnabled { get { return !Disabled && fogEnabled; } }

        public bool ExploreMapEnabled { get; private set; }


        public int Hash { get; private set; }

        public Shroud(Actor self,ShroudInfo info)
        {
            this.self = self;
            this.info = info;
            map = this.self.World.Map;

            visibleCount = new CellLayer<short>(map);
            generatedShroudCount = new CellLayer<short>(map);
            explored = new CellLayer<bool>(map);
        }
        void INotifyCreated.Created(Actor self)
        {

        }
    }
}