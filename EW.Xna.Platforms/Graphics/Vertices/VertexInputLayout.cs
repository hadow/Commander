using System;
using System.Diagnostics;
namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract partial class VertexInputLayout:IEquatable<VertexInputLayout>
    {

        protected VertexDeclaration[] VertexDeclarations { get; private set; }

        protected int[] InstanceFrequencies { get; private set; }

        /// <summary>
        /// Gets or Sets the number of used input slots.
        /// </summary>
        public int Count { get; protected set; }


        protected VertexInputLayout(int maxVertexBufferSlots):this(new VertexDeclaration[maxVertexBufferSlots],new int[maxVertexBufferSlots], 0) { }


        protected VertexInputLayout(VertexDeclaration[] vertexDeclarations,int[] instanceFrequencies,int count)
        {
            Count = count;
            VertexDeclarations = vertexDeclarations;
            InstanceFrequencies = instanceFrequencies;
        }


        public bool Equals(VertexInputLayout other)
        {

            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (Count != other.Count)
                return false;

            for(int i = 0; i < Count; i++)
            {
                if (!VertexDeclarations[i].Equals(other.VertexDeclarations[i]))
                    return false;
            }

            for(int i = 0; i < Count; i++)
            {
                if (!InstanceFrequencies[i].Equals(other.InstanceFrequencies[i]))
                    return false;
            }

            return true;
        }


        public static bool operator ==(VertexInputLayout left,VertexInputLayout right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VertexInputLayout left,VertexInputLayout right)
        {
            return !Equals(left, right);
        }

    }
}