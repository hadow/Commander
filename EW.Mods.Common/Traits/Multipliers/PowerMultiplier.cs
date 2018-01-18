using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PowerMultiplierInfo : ConditionalTraitInfo
    {
        [FieldLoader.Require]
        public readonly int Modifier = 100;



        public override object Create(ActorInitializer init)
        {
            return new PowerMultiplier(init.Self, this);
        }
    }
    public class PowerMultiplier:ConditionalTrait<PowerMultiplierInfo>,IPowerModifier,INotifyOwnerChanged
    {
        PowerManager power;

        public PowerMultiplier(Actor self,PowerMultiplierInfo info) : base(info)
        {
            power = self.Owner.PlayerActor.Trait<PowerManager>();
        }

        int IPowerModifier.GetPowerModifier(){
            return IsTraitDisabled ? 100 : Info.Modifier;
        }

        protected override void TraitDisabled(Actor self)
        {
            power.UpdateActor(self);
        }

        protected override void TraitEnabled(Actor self)
        {
            power.UpdateActor(self);
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self,Player oldOwner,Player newOwner)
        {

            power = newOwner.PlayerActor.Trait<PowerManager>();

        }

    }
}