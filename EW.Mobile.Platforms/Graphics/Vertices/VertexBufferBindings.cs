using System;
using System.Diagnostics;
namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class VertexBufferBindings:VertexInputLayout
    {
        private readonly VertexBuffer[] _vertexBuffers;
        private readonly int[] _vertexOffsets;


        public VertexBufferBindings(int maxVertexBufferSlots):base(new VertexDeclaration[maxVertexBufferSlots],new int[maxVertexBufferSlots], 0)
        {
            _vertexBuffers = new VertexBuffer[maxVertexBufferSlots];
            _vertexOffsets = new int[maxVertexBufferSlots];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public VertexBufferBinding Get(int slot)
        {
            return new VertexBufferBinding(_vertexBuffers[slot], _vertexOffsets[slot], InstanceFrequencies[slot]);
        }

    }
}