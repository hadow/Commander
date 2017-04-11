using System;
using EW.Traits;

namespace EW.Mods.Common
{
    public class FacingInit : IActorInit<int>
    {
        readonly int value = 128;

        public FacingInit()
        {

        }
        public FacingInit(int init) { value = init; }

        public int Value(World world) { return value; }
    }



    public class SubCellInit : IActorInit<SubCell>
    {
        readonly int value = (int)SubCell.FullCell;

        public SubCellInit() { }

        public SubCellInit(int init) { value = init; }

        public SubCellInit(SubCell init) { value = (int)init; }

        public SubCell Value(World world) { return (SubCell)value; }
    }

    public class LocationInit : IActorInit<CellPos>
    {
        readonly CellPos value = CellPos.Zero;

        public LocationInit() { }

        public LocationInit(CellPos init) { value = init; }

        public CellPos Value(World world) { return value; }
    }
}