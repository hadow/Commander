using System;
using System.Diagnostics;
namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// Stores the vertex buffers to be bound to the input assembler stage
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
        /// Gets vertex buffer bound to the specified input slots.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public VertexBufferBinding Get(int slot)
        {
            return new VertexBufferBinding(_vertexBuffers[slot], _vertexOffsets[slot], InstanceFrequencies[slot]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertexBufferBindings"></param>
        /// <returns></returns>
        public bool Set(params VertexBufferBinding[] vertexBufferBindings)
        {
            bool isDirty = false;

            for(int i = 0; i < vertexBufferBindings.Length; i++)
            {
                if(InstanceFrequencies[i] == vertexBufferBindings[i].InstanceFrequency 
                    && _vertexBuffers[i] == vertexBufferBindings[i].VertexBuffer 
                    && _vertexOffsets[i] == vertexBufferBindings[i].VertexOffset)
                {
                    continue;
                }

                VertexDeclarations[i] = vertexBufferBindings[i].VertexBuffer.VertexDeclaration;
                InstanceFrequencies[i] = vertexBufferBindings[i].InstanceFrequency;
                _vertexBuffers[i] = vertexBufferBindings[i].VertexBuffer;
                _vertexOffsets[i] = vertexBufferBindings[i].VertexOffset;
                isDirty = true;
            }

            if (Count > vertexBufferBindings.Length)
            {
                int startIndex = vertexBufferBindings.Length;
                int length = Count - startIndex;

                Array.Clear(VertexDeclarations, startIndex, length);
                Array.Clear(InstanceFrequencies, startIndex, length);
                Array.Clear(_vertexBuffers, startIndex, length);
                Array.Clear(_vertexOffsets, startIndex, length);
                isDirty = true;
            }

            Count = vertexBufferBindings.Length;
            return isDirty;
        }


        /// <summary>
        /// Clears the vertex buffer slots.
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            if (Count == 0)
                return false;

            Array.Clear(VertexDeclarations, 0, Count);
            Array.Clear(InstanceFrequencies, 0, Count);
            Array.Clear(_vertexBuffers, 0, Count);
            Array.Clear(_vertexOffsets, 0, Count);
            Count = 0;
            return true;
        }

    }
}