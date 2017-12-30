using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Derive facings from sprite body sequence.
    /// </summary>
    public class QuantizeFacingsFromSequenceInfo:ConditionalTraitInfo,Requires<RenderSpritesInfo>{

        [SequenceReference]
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
