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
        enum ShroudCellType : byte { Shroud,Fog,Visible}
        readonly Actor self;
        readonly ShroudInfo info;
        readonly Map map;

        readonly CellLayer<short> visibleCount;
        readonly CellLayer<short> generatedShroudCount;
        readonly CellLayer<bool> explored;

        //Per-cell cache of the resolved cell type (shroud/fog/visible)
        readonly CellLayer<ShroudCellType> resolvedType;

        [Sync]
        bool disabled;

        public event Action<IEnumerable<PPos>> CellsChanged;

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

            resolvedType = new CellLayer<ShroudCellType>(map);
        }


        void INotifyCreated.Created(Actor self)
        {

        }

        public bool IsVisible(CPos cell)
        {
            return IsVisible(cell.ToMPos(map));
        }

        public bool IsVisible(MPos uv)
        {
            if (!resolvedType.Contains(uv))
                return false;

            foreach (var puv in map.ProjectedCellsCovering(uv))
                if (IsVisible(puv))
                    return true;

            return false;
        }

        public bool IsVisible(WPos pos)
        {
            return IsVisible(map.ProjectedCellCovering(pos));
        }

        public bool IsVisible(PPos puv)
        {
            if (!FogEnabled)
                return map.Contains(puv);

            var uv = (MPos)puv;
            return resolvedType.Contains(uv) && resolvedType[uv] == ShroudCellType.Visible;
        }


        public bool IsExplored(PPos puv)
        {
            if (Disabled)
                return map.Contains(puv);

            var uv = (MPos)puv;
            return resolvedType.Contains(uv) && resolvedType[uv] > ShroudCellType.Shroud;
        }
    }
}