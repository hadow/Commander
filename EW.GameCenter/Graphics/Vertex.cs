using System.Runtime.InteropServices;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;

namespace EW.Graphics
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex:IVertexT
	{
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 UV;
        public float Palette;
        public float C;

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexT.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        static Vertex()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0,VertexElementFormat.Vector3,VertexElementUsage.Position,0),
                new VertexElement(12,VertexElementFormat.Vector2,VertexElementUsage.TextureCoordinate,1),
                new VertexElement(20,VertexElementFormat.Vector2,VertexElementUsage.TextureCoordinate,2),
                new VertexElement(28,VertexElementFormat.Single,VertexElementUsage.Depth,3),
                new VertexElement(32,VertexElementFormat.Color,VertexElementUsage.Color,4),
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }

    }


}
