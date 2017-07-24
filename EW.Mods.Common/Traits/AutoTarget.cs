using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class AutoTargetInfo : ITraitInfo,Requires<AttackBaseInfo>,UsesInit<StanceInit>
    {
        public readonly bool AllowMovement = true;

        public readonly int ScanRadius = -1;        //set to a value>1 to override weapons maimum range for this.

        public readonly UnitStance InitialStanceAI = UnitStance.AttackAnything;

        public readonly UnitStance InitialStance = UnitStance.Defend;

        public readonly bool EnableStance = true;

        public readonly int MinimumScanTimeInterval = 3;

        public readonly int MaximumScanTimeInterval = 8;

        public readonly bool TargetWhenIdle = true;

        public readonly bool TargetWhenDamaged = true;


        public object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }
    public class AutoTarget
    {
    }


    class AutoTargetIgnoreInfo : TraitInfo<AutoTargetIgnore>
    {

    }

    class AutoTargetIgnore
    {

    }

    public enum UnitStance { HoldFire,ReturnFire,Defend,AttackAnything}
    public class StanceInit : IActorInit<UnitStance>
    {
        [FieldLoader.FieldFromYamlKey]
        readonly UnitStance value = UnitStance.AttackAnything;

        public StanceInit() { }

        public StanceInit(UnitStance init)
        {
            value = init;
        }

        public UnitStance Value(World world) { return value; }
    }
}