using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class LineBuildSegmentExternalConditionInfo:ConditionalTraitInfo,Requires<LineBuildInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new LineBuildSegmentExternalCondition(init.Self, this);
        }

    }

    public class LineBuildSegmentExternalCondition:ConditionalTrait<LineBuildSegmentExternalConditionInfo>
    {
        public LineBuildSegmentExternalCondition(Actor self,LineBuildSegmentExternalConditionInfo info) : base(info)
        {

        }
    }
}