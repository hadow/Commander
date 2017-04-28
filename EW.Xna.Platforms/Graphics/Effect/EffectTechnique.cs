using System;
using System.Collections.Generic;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class EffectTechnique
    {

        public string Name { get; private set; }

        public EffectPassCollection Passes { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }


        internal EffectTechnique(Effect effect,string name,EffectPassCollection passes,EffectAnnotationCollection annotations)
        {
            Name = name;
            Passes = passes;
            Annotations = annotations;

        }

        internal EffectTechnique(Effect effect,EffectTechnique cloneSource)
        {
            Name = cloneSource.Name;
            Annotations = cloneSource.Annotations;


        }
    }
}