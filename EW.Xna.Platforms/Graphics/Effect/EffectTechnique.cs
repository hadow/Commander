using System;
using System.Collections.Generic;


namespace EW.Xna.Platforms.Graphics
{
    public class EffectTechnique
    {

        public string Name { get; private set; }

        public EffectPassCollection Passes { get; private set; }

        internal EffectTechnique(Effect effect,string name,EffectPassCollection passes)
        {
            Passes = passes;

        }
    }
}