using System;

namespace RA.Mobile.Platforms.Graphics
{
    public enum VertexElementUsage
    {
        Position,           //位置
        Color,              //颜色
        TextureCoordinate,//纹理坐标
        Normal,             //法线
        Tangent,            //切线
        Depth,              //深度

    }

    /// <summary>
    /// 
    /// </summary>
    public enum VertexElementFormat
    {
        Single,
        Vector2,
        Vector3,
        Vector4,
        Bolor,
        Byte4,
        Short2,//tow signed 16-bit integer
        Short4,
        NormalizedShort2,
        NormalizedShort4,

    }

    /// <summary>
    /// 
    /// </summary>
    public struct VertexElement:IEquatable<VertexElement>
    {

        private int _offset;
        public int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        private VertexElementFormat _format;
        public VertexElementFormat VertexElementFormat
        {
            get { return _format; }
            set { _format = value; }
        }

        private VertexElementUsage _usage;

        public VertexElementUsage VertexElementUsage
        {
            get { return _usage; }
            set { _usage = value; }
        }

        private int _usageIndex;

        public int UsageIndex
        {
            get { return _usageIndex; }
            set { _usageIndex = value; }
        }

        public VertexElement(int offset,VertexElementFormat elementFormat,VertexElementUsage elementUsage,int usageIndex)
        {
            _offset = offset;
            _format = elementFormat;
            _usageIndex = usageIndex;
            _usage = elementUsage;
        }


    }
}