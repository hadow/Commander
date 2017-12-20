using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithTextDecorationInfo:ConditionalTraitInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new WithTextDecoration(init.Self, this);
        }
    }

    public class WithTextDecoration : ConditionalTrait<WithTextDecorationInfo>
    {
        public WithTextDecoration(Actor self,WithTextDecorationInfo info) : base(info)
        {

        }
    }
}