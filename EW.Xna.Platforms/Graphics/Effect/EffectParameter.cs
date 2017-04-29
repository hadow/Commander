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

    /// <summary>
    /// 
    /// </summary>
    public class EffectParameter
    {

        internal static ulong NextStateKey { get; private set; }

        public string Name { get; private set; }

        public string Semantic { get; private set; }

        public EffectParameterClass ParameterClass { get; private set; }

        public EffectParameterType ParameterType { get; private set; }


        public int RowCount { get; private set; }

        public int ColumnCount { get; private set; }

        public EffectParameterCollection Elements { get; private set; }

        public EffectParameterCollection StructureMembers { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

        internal object Data { get; private set; }

        internal ulong StateKey { get; private set; }
        internal EffectParameter(EffectParameter cloneSource)
        {

            //immutable types
            ParameterClass = cloneSource.ParameterClass;
            ParameterType = cloneSource.ParameterType;
            Name = cloneSource.Name;
            Semantic = cloneSource.Semantic;
            Annotations = cloneSource.Annotations;
            RowCount = cloneSource.RowCount;
            ColumnCount = cloneSource.ColumnCount;

            //mutable types
            Elements = cloneSource.Elements.Clone();
            StructureMembers = cloneSource.StructureMembers.Clone();

            var array = cloneSource.Data as Array;
            if (array != null)
                Data = array.Clone();
            StateKey = unchecked(NextStateKey++);
            

        }

    }
}