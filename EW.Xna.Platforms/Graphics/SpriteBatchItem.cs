using System;


namespace EW.Xna.Platforms.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    internal class SpriteBatchItem:IComparable<SpriteBatchItem>
    {
        public float SortKey;

        public Texture2D Texture;

        public VertexPositionColorTexture vertexTL;
        public VertexPositionColorTexture vertexTR;
        public VertexPositionColorTexture vertexBL;
        public VertexPositionColorTexture vertexBR;

        public SpriteBatchItem()
        {
            vertexBL = new VertexPositionColorTexture();
            vertexBR = new VertexPositionColorTexture();
            vertexTL = new VertexPositionColorTexture();
            vertexTR = new VertexPositionColorTexture();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="color"></param>
        /// <param name="texCoordTL"></param>
        /// <param name="texCoordBR"></param>
        /// <param name="depth"></param>
        public void Set(float x,float y,float w,float h,Color color,Vector2 texCoordTL,Vector2 texCoordBR,float depth)
        {
            vertexTL.Position.X = x;
            vertexTL.Position.Y = y;
            vertexTL.Position.Z = depth;
            vertexTL.Color = color;
            vertexTL.TextureCoordinate.X = texCoordTL.X;
            vertexTL.TextureCoordinate.Y = texCoordTL.Y;

            vertexTR.Position.X = x + w;
            vertexTR.Position.Y = y;
            vertexTR.Position.Z = depth;
            vertexTR.Color = color;
            vertexTR.TextureCoordinate.X = texCoordBR.X;
            vertexTR.TextureCoordinate.Y = texCoordTL.Y;

            vertexBL.Position.X = x;
            vertexBL.Position.Y = y + h;
            vertexBL.Position.Z = depth;
            vertexBL.Color = color;
            vertexBL.TextureCoordinate.X = texCoordTL.X;
            vertexBL.TextureCoordinate.Y = texCoordBR.Y;

            vertexBR.Position.X = x + w;
            vertexBR.Position.Y = y + h;
            vertexBR.Position.Z = depth;
            vertexBR.Color = color;
            vertexBR.TextureCoordinate.X = texCoordBR.X;
            vertexBR.TextureCoordinate.Y = texCoordBR.Y;
        }


        public int CompareTo(SpriteBatchItem other)
        {
            return SortKey.CompareTo(other.SortKey);
        }

    }
}