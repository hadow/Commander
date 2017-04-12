using System;

namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public struct VertexBufferBinding
    {


        private readonly VertexBuffer _vertexBuffer;

        public VertexBuffer VertexBuffer { get { return _vertexBuffer; } }

        private readonly int _vertexOffset;

        public int VertexOffset { get { return _vertexOffset; } }

        private readonly int _instanceFrequency;



        public VertexBufferBinding(VertexBuffer vertexBuffer,int vertexOffset,int instanceFrequency)
        {

            _vertexBuffer = vertexBuffer;
            _vertexOffset = vertexOffset;
            _instanceFrequency = instanceFrequency;
        }
    }
}