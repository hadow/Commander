using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class AutoTargetInfo : UpgradableTraitInfo,Requires<AttackBaseInfo>,UsesInit<StanceInit>
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


        public override object Create(ActorInitializer init)
        {
            return new AutoTarget(init, this);
        }
    }
    public class AutoTarget:UpgradableTrait<AutoTargetInfo>
    {

        public AutoTarget(ActorInitializer init,AutoTargetInfo info) : base(info)
        {

        }
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
        [FieldFromYamlKey]
        readonly UnitStance value = UnitStance.AttackAnything;

        public StanceInit() { }

        public StanceInit(UnitStance init)
        {
            value = init;
        }

        public UnitStance Value(World world) { return value; }
    }
}