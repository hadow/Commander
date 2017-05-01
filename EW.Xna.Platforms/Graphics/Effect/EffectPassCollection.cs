using System;
using System.Collections;
using System.Collections.Generic;

namespace EW.Xna.Platforms.Graphics
{
    public class EffectPassCollection:IEnumerable<EffectPass>
    {
        private readonly EffectPass[] _passes;

        internal EffectPassCollection(EffectPass[] passes)
        {
            _passes = passes;
        }

        internal EffectPassCollection Clone(Effect effect)
        {
            var passes = new EffectPass[_passes.Length];
            for (var i = 0; i < _passes.Length; i++)
                passes[i] = new EffectPass(effect,_passes[i]);

            return new EffectPassCollection(passes);
        }
        public EffectPass this[int index]
        {
            get { return _passes[index]; }
        }


        public EffectPass this[string name]
        {
            get
            {
                foreach(var pass in _passes)
                {
                    if (pass.Name == name)
                        return pass;
                }
                return null;
            }
        }

        IEnumerator<EffectPass>  IEnumerable<EffectPass>.GetEnumerator()
        {
            return ((IEnumerable<EffectPass>)_passes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _passes.GetEnumerator();
        }


    }
}