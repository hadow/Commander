using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
using EW.Xna.Platforms.Graphics;
namespace EW.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TerrainSpriteLayer:IDisposable
    {
        public bool restrictToBounds;

        public readonly Sheet Sheet;


        public readonly BlendMode BlendMode;

        readonly Sprite emptySprite;

        readonly WorldRenderer worldRender;

        readonly Map map;

        readonly PaletteReference palette;

        readonly VertexBuffer vertexBuffer;

        readonly HashSet<int> dirtyRows = new HashSet<int>();

        readonly int rowStride;

        public TerrainSpriteLayer(World world,WorldRenderer wr,Sheet sheet,BlendMode blendMode,PaletteReference palette,bool restrictToBounds)
        {
            worldRender = wr;
            this.restrictToBounds = restrictToBounds;
            Sheet = sheet;
            BlendMode = blendMode;
            this.palette = palette;

            map = world.Map;
            rowStride = 6 * map.MapSize.X;

            var vertexCount = rowStride * map.MapSize.Y;

            vertexBuffer = new VertexBuffer(GraphicsDeviceManager.M.GraphicsDevice, typeof(VertexPositionColorTexture), vertexCount, BufferUsage.None);


            emptySprite = new Sprite(sheet, Rectangle.Empty, TextureChannel.Alpha);

            wr.PaletteInvalidated += UpdatePaletteIndices;

            
        }

        /// <summary>
        /// /
        /// </summary>
        void UpdatePaletteIndices()
        {
            for(var row = 0; row < map.MapSize.Y; row++)
            {
                dirtyRows.Add(row);
            }
        }

        public void Update(MPos uv,Sprite sprite,Vector3 pos)
        {

        }

        public void Update(CPos cell,Sprite sprite)
        {

        }



        public void Draw(GameViewPort viewport)
        {

        }

        public void Dispose()
        {

        }


    }
}