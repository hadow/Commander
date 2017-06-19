using System;
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

        internal EffectParameter(EffectParameterClass class_,
            EffectParameterType type,
            string name,
            int rowCount,
            int columnCount,
            string semantic,
            EffectAnnotationCollection annotations,
            EffectParameterCollection elements,
            EffectParameterCollection structMembers,
            object data)
        {
            ParameterClass = class_;
            ParameterType = type;

            Name = name;
            Semantic = semantic;

            Annotations = annotations;
            RowCount = rowCount;
            ColumnCount = columnCount;

            Elements = elements;
            StructureMembers = structMembers;

            Data = data;

            StateKey = unchecked(NextStateKey++);
        }


        public void SetValue(bool value)
        {
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Bool)
                throw new InvalidCastException();

#if OPENGL

            ((float[])Data)[0] = value ? 1 : 0;
#else
#endif

            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Matrix value)
        {
            if (ParameterClass != EffectParameterClass.Matrix || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException("Matrix");

            if (RowCount == 4 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;

                fData[12] = value.M14;
                fData[13] = value.M24;
                fData[14] = value.M34;
                fData[15] = value.M44;
            }
            else if (RowCount == 4 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;
            }
            else if (RowCount == 3 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;

                fData[9] = value.M14;
                fData[10] = value.M24;
                fData[11] = value.M34;
            }
            else if (RowCount == 3 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;
            }
            else if (RowCount == 3 && ColumnCount == 2)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;
            }

            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Texture value)
        {
            if(this.ParameterType != EffectParameterType.Texture &&
                this.ParameterType != EffectParameterType.Texture1D&&
                this.ParameterType != EffectParameterType.Texture2D)
            {
                throw new InvalidCastException();
            }

            Data = value;
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Single value)
        {
            if (ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();
            ((float[])Data)[0] = value;
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Vector2 value)
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            StateKey = unchecked(NextStateKey++);
        }


        public void SetValue(Vector3 value)
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Vector4 value)
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            fData[3] = value.W;
            StateKey = unchecked(NextStateKey++);
        }

    }
}