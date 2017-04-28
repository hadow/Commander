using System;
using System.Collections;
using System.Collections.Generic;

namespace EW.Xna.Platforms.Graphics
{
    public class EffectAnnotationCollection:IEnumerable<EffectAnnotation>
    {
        internal static EffectAnnotationCollection Empty = new EffectAnnotationCollection(new EffectAnnotation[0]);

        private readonly EffectAnnotation[] _annotations;

        internal EffectAnnotationCollection(EffectAnnotation[] annotations)
        {
            _annotations = annotations;
        }

        public int Count { get { return _annotations.Length; } }


        public EffectAnnotation this[int index]
        {
            get { return _annotations[index]; }
        }

        public EffectAnnotation this[string name]
        {
            get
            {
                foreach(var annotation in _annotations)
                {
                    if (annotation.Name == name)
                        return annotation;
                }
                return null;
            }
        }

        public IEnumerator<EffectAnnotation> GetEnumerator()
        {
            return ((IEnumerable<EffectAnnotation>)_annotations).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _annotations.GetEnumerator();
        }


    }
}