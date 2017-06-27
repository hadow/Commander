using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class QuantizeFacingsFromSequenceInfo:ConditionalTraitInfo,Requires<RenderSpritesInfo>{


        public readonly string Sequence = "idle";

        public override object Create(ActorInitializer init)
        {
            return new QuantizeFacingsFromSequence(this);
        }
    }

    public class QuantizeFacingsFromSequence
    {
        public QuantizeFacingsFromSequence(QuantizeFacingsFromSequenceInfo info)
        {
        }
    }
}
