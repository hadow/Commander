using System;
using System.Collections;
using System.Collections.Generic;


namespace EW.Xna.Platforms.Graphics
{
    public class EffectTechniqueCollection:IEnumerable<EffectTechnique>
    {

        private readonly EffectTechnique[] _techniques;

        internal EffectTechniqueCollection(EffectTechnique[] techniques)
        {
            _techniques = techniques;
        }


        public IEnumerator<EffectTechnique> GetEnumerator()
        {
            return ((IEnumerable<EffectTechnique>)_techniques).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _techniques.GetEnumerator();
        }
        
    }
}