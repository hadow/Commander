using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public enum UncloakType
    {
        None = 0,
        Attack = 1,
        Move = 2,
        Unload = 4,
        Infiltrate = 8,//潜入
        Demolish = 16,//拆除，破坏
        Damage = 32,
        Heal = 64,
        SelfHeal = 128,
        Dock = 256,//码头,船坞
    }


    /// <summary>
    /// This unit can be cloak and uncloak in specific situations.
    /// </summary>
    public class CloakInfo : ConditionalTraitInfo
    {
        public readonly int InitialDelay = 10;

        public readonly int CloakDelay = 30;

        public readonly UncloakType UncloakOn = UncloakType.Attack | UncloakType.Unload | UncloakType.Infiltrate | UncloakType.Demolish | UncloakType.Dock;

        public readonly string CloakSound = null;
        public readonly string UncloakSound = null;

        public readonly HashSet<string> CloakTypes = new HashSet<string> { "Cloak" };



        public override object Create(ActorInitializer init)
        {
            return new Cloak(this);
        }
    }




    public class Cloak:ConditionalTrait<CloakInfo>
    {


        public Cloak(CloakInfo info) : base(info) { }
    }
}