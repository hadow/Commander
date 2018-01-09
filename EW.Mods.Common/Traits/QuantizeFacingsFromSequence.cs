using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Derive facings from sprite body sequence.
    /// </summary>
    public class QuantizeFacingsFromSequenceInfo:ConditionalTraitInfo,IQuantizeBodyOrientationInfo,Requires<RenderSpritesInfo>{

        [SequenceReference]
        public readonly string Sequence = "idle";
        
        public int QuantizedBodyFacings(ActorInfo ai,SequenceProvider sequenceProvider,string race)
        {
            if (string.IsNullOrEmpty(Sequence))
                throw new InvalidOperationException("Actor " + ai.Name + " is missing sequence to quantize facings from.");

            var rsi = ai.TraitInfo<RenderSpritesInfo>();
            return sequenceProvider.GetSequence(rsi.GetImage(ai, sequenceProvider, race), Sequence).Facings;
        }
        public override object Create(ActorInitializer init)
        {
            return new QuantizeFacingsFromSequence(this);
        }
    }

    public class QuantizeFacingsFromSequence:ConditionalTrait<QuantizeFacingsFromSequenceInfo>
    {
        public QuantizeFacingsFromSequence(QuantizeFacingsFromSequenceInfo info) : base(info) { }
    }
}
