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

        internal EffectTechniqueCollection Clone(Effect effect)
        {
            var techniques = new EffectTechnique[_techniques.Length];
            for (var i = 0; i < _techniques.Length; i++)
                techniques[i] = new EffectTechnique(effect, _techniques[i]);

            return new EffectTechniqueCollection(techniques);
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