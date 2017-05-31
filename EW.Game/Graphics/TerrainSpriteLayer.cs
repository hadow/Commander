using System;
using System.IO;
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

        readonly DynamicVertexBuffer vertexBuffer;

        readonly HashSet<int> dirtyRows = new HashSet<int>();

        readonly int rowStride;

        readonly VertexPositionColorTexture[] vertices;

        public TerrainSpriteLayer(World world,WorldRenderer wr,Sheet sheet,BlendMode blendMode,PaletteReference palette,bool restrictToBounds)
        {
            worldRender = wr;
            this.restrictToBounds = restrictToBounds;
            Sheet = sheet;
            BlendMode = blendMode;
            this.palette = palette;

            map = world.Map;
            rowStride = 6 * map.MapSize.X;

            vertices = new VertexPositionColorTexture[rowStride * map.MapSize.Y];

            var vertexCount = rowStride * map.MapSize.Y;

            vertexBuffer = new DynamicVertexBuffer(GraphicsDeviceManager.M.GraphicsDevice, typeof(VertexPositionColorTexture), vertexCount, BufferUsage.None);


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

            if (sprite != null)
            {
                if (sprite.Sheet != Sheet)
                    throw new InvalidDataException("Attempted to add sprite from a different sheet");
                if (sprite.BlendMode != BlendMode)
                    throw new InvalidDataException("Attempted to add sprite with a different blend mode");


            }
            else
                sprite = emptySprite;

            var offset = rowStride * uv.V + 6 * uv.U;
            //todo

            dirtyRows.Add(uv.V);


        }

        public void Update(CPos cell,Sprite sprite)
        {
            var xyz = sprite == null ? Vector3.Zero : worldRender.Screen3DPosition(map.CenterOfCell(cell)) + sprite.Offset -  sprite.Size*0.5f;

            Update(cell.ToMPos(map.Grid.Type), sprite, xyz);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewport"></param>
        public void Draw(GameViewPort viewport)
        {
            var cells = restrictToBounds ? viewport.VisibleCellsInsideBounds : viewport.AllVisibleCells;

            //only draw the rows that are visible
            var firstRow = cells.CandidateMapCoords.TopLeft.V.Clamp(0, map.MapSize.Y);
            var lastRow = (cells.CandidateMapCoords.BottomRight.V + 1).Clamp(firstRow, map.MapSize.Y);

            //Flush any visible changes to the GPU
            for(var row = firstRow; row <= lastRow; row++)
            {
                if (!dirtyRows.Remove(row))
                    continue;

                var rowOffset = rowStride * row;
                vertexBuffer.SetData(System.Runtime.InteropServices.Marshal.SizeOf<VertexPositionColorTexture>()*rowOffset,vertices,rowOffset,rowStride,0,SetDataOptions.None);
            }
        }

        public void Dispose()
        {
            worldRender.PaletteInvalidated -= UpdatePaletteIndices;
            vertexBuffer.Dispose();
        }


    }
}