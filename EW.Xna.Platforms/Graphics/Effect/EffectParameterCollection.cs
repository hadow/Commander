using System;
using System.Collections;
using System.Collections.Generic;


namespace EW.Xna.Platforms.Graphics
{
    public class EffectParameterCollection:IEnumerable<EffectParameter>
    {

        internal static readonly EffectParameterCollection Empty = new EffectParameterCollection(new EffectParameter[0]);
        private readonly EffectParameter[] _parameters;

        internal EffectParameterCollection(EffectParameter[] parameters)
        {
            _parameters = parameters;
        }

        public int Count { get { return _parameters.Length; } }

        public EffectParameter this[int index] { get { return _parameters[index]; } }

        public EffectParameter this[string name]
        {
            get
            {
                foreach(var parameter in _parameters)
                {
                    if (parameter.Name == name)
                        return parameter;
                }
                return null;
            }
        }

        internal EffectParameterCollection Clone()
        {
            if (_parameters.Length == 0)
                return Empty;

            var parameters = new EffectParameter[_parameters.Length];
            for(var i = 0; i < parameters.Length; i++)
            {
                parameters[i] = new EffectParameter(_parameters[i]);
            }

            return new EffectParameterCollection(parameters);
        }

        public IEnumerator<EffectParameter> GetEnumerator()
        {
            return ((IEnumerable<EffectParameter>)_parameters).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }


    }
}