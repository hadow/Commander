using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;

namespace EW.Graphics
{

    
	[StructLayout(LayoutKind.Sequential,Pack = 1)]
	public struct Vertex:IVertexT
	{
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 UV;
        public float Palette;
        public float C;

        public static readonly VertexDeclaration VertexDeclaration;


        public Vertex(Vector3 pos,Vector2 texCoordinate,Vector2 uv,float palette,float c)
        {
            this.Position = pos;
            this.TextureCoordinate = texCoordinate;
            this.UV = uv;
            this.Palette = palette;
            this.C = c;
        }

        VertexDeclaration IVertexT.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        public static bool operator ==(Vertex a,Vertex b)
        {
            return a.Position == b.Position && a.TextureCoordinate == b.TextureCoordinate && a.UV == b.UV && a.Palette == b.Palette && a.C == b.C;
        }

        public static bool operator !=(Vertex a,Vertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (base.GetType() != obj.GetType())
                return false;

            return (this == (Vertex)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {

                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
                hashCode = (hashCode * 397) ^ UV.GetHashCode();
                hashCode = (hashCode * 397) ^ Palette.GetHashCode();
                hashCode = (hashCode * 397) ^ C.GetHashCode();
                return hashCode;
            }

        }

        static Vertex()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0,VertexElementFormat.Vector3,VertexElementUsage.Position,0),
                new VertexElement(12,VertexElementFormat.Vector2,VertexElementUsage.TextureCoordinate,0),
                new VertexElement(20,VertexElementFormat.Vector2,VertexElementUsage.Normal,0),
                new VertexElement(28,VertexElementFormat.Single,VertexElementUsage.Depth,0),
                new VertexElement(32,VertexElementFormat.Color,VertexElementUsage.Color,0),
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }

    }


}
