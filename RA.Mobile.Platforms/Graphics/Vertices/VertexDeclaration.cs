using System;
using System.Collections.Generic;

namespace RA.Mobile.Platforms.Graphics
{
    /// <summary>
    /// Define per-vertex data of a vertex buffer
    /// </summary>
    public partial class VertexDeclaration:GraphicsResource,IEquatable<VertexDeclaration>
    {
        private sealed class Data : IEquatable<Data>
        {
            private readonly int _hashCode;
            public readonly int VertexStride;//¶¥µã²½½ø
            public VertexElement[] Elements;

            public Data(int vertexStride,VertexElement[] elements)
            {
                VertexStride = vertexStride;
                Elements = elements;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        private static readonly Dictionary<Data, VertexDeclaration> _vertexDeclarationCache;
        private readonly Data _data;
        static VertexDeclaration()
        {
            _vertexDeclarationCache = new Dictionary<Data, VertexDeclaration>();
        }

        private VertexDeclaration(Data data)
        {
            _data = data;
        }


        public int VertexStride
        {
            get { return _data.VertexStride; }
        }

        internal VertexElement[] InternalVertexElements
        {
            get { return _data.Elements; }
        }

    }
}