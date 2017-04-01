using System;
using System.Diagnostics;
namespace EW.Mobile.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract partial class VertexInputLayout:IEquatable<VertexInputLayout>
    {

        protected VertexDeclaration[] VertexDeclarations { get; private set; }

        protected int[] InstanceFrequencies { get; private set; }

        public int Count { get; private set; }


        protected VertexInputLayout(int maxVertexBufferSlots):this(new VertexDeclaration[maxVertexBufferSlots],new int[maxVertexBufferSlots], 0) { }


        protected VertexInputLayout(VertexDeclaration[] vertexDeclarations,int[] instanceFrequencies,int count)
        {
            Count = count;
            VertexDeclarations = vertexDeclarations;
            InstanceFrequencies = instanceFrequencies;
        }


        public bool Equals(VertexInputLayout other)
        {
            return true;
        }



    }
}