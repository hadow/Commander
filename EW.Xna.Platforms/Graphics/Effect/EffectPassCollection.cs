using System;
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

        public EffectPass this[int index]
        {
            get { return _passes[index]; }
        }

    }
}