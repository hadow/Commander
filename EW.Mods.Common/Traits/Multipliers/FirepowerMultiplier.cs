using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class FirepowerMultiplierInfo : ConditionalTraitInfo
    {

        [FieldLoader.Require]
        public readonly int Modifier = 100;
        public override object Create(ActorInitializer init)
        {
            return new FirepowerMultiplier(this);
        }
    }

    public class FirepowerMultiplier:ConditionalTrait<FirepowerMultiplierInfo>,IFirepowerModifier
    {

        public FirepowerMultiplier(FirepowerMultiplierInfo info) : base(info) { }

        int IFirepowerModifier.GetFirepowerModifier(){
            return IsTraitDisabled ? 100 : Info.Modifier;
        }
    }
}