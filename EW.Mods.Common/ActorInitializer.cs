using System;
using EW.Traits;

namespace EW.Mods.Common
{
    public class FacingInit : IActorInit<int>
    {
        [FieldLoader.FieldFromYamlKey]
        readonly int value = 128;

        public FacingInit()
        {

        }
        public FacingInit(int init) { value = init; }

        public int Value(World world) { return value; }
    }



    public class SubCellInit : IActorInit<SubCell>
    {
        [FieldLoader.FieldFromYamlKey]
        readonly int value = (int)SubCell.FullCell;

        public SubCellInit() { }

        public SubCellInit(int init) { value = init; }

        public SubCellInit(SubCell init) { value = (int)init; }

        public SubCell Value(World world) { return (SubCell)value; }
    }

    //public class LocationInit : IActorInit<CPos>
    //{
    //    readonly CPos value = CPos.Zero;

    //    public LocationInit() { }

    //    public LocationInit(CPos init) { value = init; }

    //    public CPos Value(World world) { return value; }
    //}
}