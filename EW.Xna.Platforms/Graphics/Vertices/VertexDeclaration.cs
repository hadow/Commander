using System;
using System.Collections.Generic;

namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// Define per-vertex data of a vertex buffer
    /// </summary>
    public partial class VertexDeclaration:GraphicsResource,IEquatable<VertexDeclaration>
    {
        /// <summary>
        /// 在结构上相同的顶点声明之间共享数据
        /// </summary>
        private sealed class Data : IEquatable<Data>
        {
            private readonly int _hashCode;
            public readonly int VertexStride;//顶点跨距
            public VertexElement[] Elements;

            public Data(int vertexStride,VertexElement[] elements)
            {
                VertexStride = vertexStride;
                Elements = elements;

                //预先计算Hash Code,方便后面快速比较和字典查询
                unchecked
                {
                    _hashCode = elements[0].GetHashCode();
                    for (int i = 0; i < elements.Length; i++)
                        _hashCode = (_hashCode * 397) ^ elements[i].GetHashCode();

                    _hashCode = (_hashCode * 397) ^ elements.Length;
                    _hashCode = (_hashCode * 397) ^ vertexStride;
                }
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Data);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            public bool Equals(Data data)
            {
                if (ReferenceEquals(null, data))
                    return false;

                if (ReferenceEquals(this, data))
                    return true;

                if (_hashCode != data._hashCode || VertexStride != data.VertexStride || Elements.Length != data.Elements.Length)
                    return false;

                for (int i = 0; i < Elements.Length; i++)
                    if (!Elements[i].Equals(data.Elements[i]))
                        return false;

                return true;
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


        public VertexDeclaration(params VertexElement[] elements) : this(GetVertexStride(elements), elements) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexStride"></param>
        /// <param name="elements"></param>
        public VertexDeclaration(int vertexStride,params VertexElement[] elements)
        {
            if (elements == null || elements.Length == 0)
                throw new ArgumentNullException("elements", "Elements cannot be empty");

            lock (_vertexDeclarationCache)
            {
                var data = new Data(vertexStride, elements);
                VertexDeclaration vertexDeclaration;
                if(_vertexDeclarationCache.TryGetValue(data,out vertexDeclaration))
                {
                    //Resuse existing data.
                    _data = vertexDeclaration._data;
                }
                else
                {
                    // Cache new vertex declaration
                    data.Elements = (VertexElement[])elements.Clone();
                    _data = data;
                    _vertexDeclarationCache[data] = this;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        private static int GetVertexStride(VertexElement[] elements)
        {
            int max = 0;
            for(var i = 0; i < elements.Length; i++)
            {
                var start = elements[i].Offset + elements[i].VertexElementFormat.GetSize();
                if (max < start)
                    max = start;
            }
            return max;
        }


        public int VertexStride
        {
            get { return _data.VertexStride; }
        }

        internal VertexElement[] InternalVertexElements
        {
            get { return _data.Elements; }
        }

        public bool Equals(VertexDeclaration other)
        {
            return false;
        }

    }
}