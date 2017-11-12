using System;
using EW.Traits;

namespace EW.Mods.Common
{
    public class FacingInit : IActorInit<int>
    {
        [FieldFromYamlKey]
        readonly int value = 128;

        public FacingInit()
        {

        }
        public FacingInit(int init) { value = init; }

        public int Value(World world) { return value; }
    }


    public class DynamicFacingInit : IActorInit<Func<int>>
    {
        readonly Func<int> func;

        public DynamicFacingInit(Func<int> func) { this.func = func; }

        public Func<int> Value(World world) { return func; }
    }


    public class SubCellInit : IActorInit<SubCell>
    {
        [FieldFromYamlKey]
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


        /// <summary>
        /// Allow maps / transformations to specify the faction variant of an acotr.
        /// </summary>
    public class FactionInit : IActorInit<string>
    {
        [FieldFromYamlKey]
        public readonly string Faction;

        public FactionInit() { }

        public FactionInit(string faction) { Faction = faction; }

        public string Value(World world) { return Faction; }
    }
}