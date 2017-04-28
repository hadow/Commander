using System;
using System.Collections.Generic;

namespace EW.Xna.Platforms.Graphics
{
    public enum EffectParameterClass
    {
        Scalar,
        Vector,
        Matrix,
        Object,
        Struct,
    }

    public enum EffectParameterType
    {
        Void,
        Bool,
        Int32,
        Single,
        String,
        Texture,
        Texture1D,
        Texture2D,
    }

    public class EffectParameter
    {

        internal static ulong NextStateKey { get; private set; }

        public string Name { get; private set; }

        public string Semantic { get; private set; }

        public EffectParameterClass ParameterClass { get; private set; }

        public EffectParameterType ParameterType { get; private set; }

    }
}