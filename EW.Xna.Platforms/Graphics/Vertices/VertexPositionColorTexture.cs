using System.Runtime.InteropServices;
namespace EW.Xna.Platforms.Graphics
{

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential,Pack =1)]
    public struct VertexPositionColorTexture:IVertexT
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexT.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }                                                                                                                                                                                                                                 

        public static bool operator ==(VertexPositionColorTexture left,VertexPositionColorTexture right)
        {
            return left.Position == right.Position && left.Color == right.Color && left.TextureCoordinate == right.TextureCoordinate;
        }
        public static bool operator !=(VertexPositionColorTexture left,VertexPositionColorTexture right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == (VertexPositionColorTexture)obj);
        }


        public override int GetHashCode()
        {
            return 0;
        }

        static VertexPositionColorTexture()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0,VertexElementFormat.Vector3,VertexElementUsage.Position,0),
                new VertexElement(12,VertexElementFormat.Color,VertexElementUsage.Color,0),
                new VertexElement(16,VertexElementFormat.Vector2,VertexElementUsage.TextureCoordinate,0),
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }







    }
}