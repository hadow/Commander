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
    public sealed class TerrainSpriteLayer:DrawableGameComponent
    {
        public bool restrictToBounds;

        public readonly Sheet Sheet;


        public readonly BlendMode BlendMode;

        readonly Sprite emptySprite;

        readonly WorldRenderer worldRender;

        readonly Map map;

        readonly PaletteReference palette;

        /// <summary>
        /// Extract terrain vertex bufer into a reusable class.
        /// </summary>
        readonly DynamicVertexBuffer vertexBuffer;

        readonly VertexBufferBinding[] bindings;
        readonly HashSet<int> dirtyRows = new HashSet<int>();

        readonly int rowStride;

        readonly Vertex[] vertices;

        public TerrainSpriteLayer(Game game,World world,WorldRenderer wr,Sheet sheet,BlendMode blendMode,PaletteReference palette,bool restrictToBounds):base(game)
        {
            worldRender = wr;
            this.restrictToBounds = restrictToBounds;
            Sheet = sheet;
            BlendMode = blendMode;
            this.palette = palette;

            map = world.Map;
            rowStride = 6 * map.MapSize.X;

            vertices = new Vertex[rowStride * map.MapSize.Y];

            var vertexCount = rowStride * map.MapSize.Y;

            this.bindings = new VertexBufferBinding[1];

            vertexBuffer = new DynamicVertexBuffer(this.GraphicsDevice, typeof(Vertex), vertexCount, BufferUsage.None);

            this.bindings[0] = new VertexBufferBinding(vertexBuffer);

            emptySprite = new Sprite(sheet, Rectangle.Empty, TextureChannel.Alpha);

            wr.PaletteInvalidated += UpdatePaletteIndices;

            
        }

        /// <summary>
        /// /
        /// </summary>
        void UpdatePaletteIndices()
        {
            //Everything in the layer uses the same palette,
            //so we can fix the indices in one pass
            for(var i= 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                vertices[i] = new Vertex(v.Position, v.TextureCoordinate, v.UV, palette.TextureIndex, v.C);
            }


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

            Util.FastCreateQuad(vertices, pos, sprite, palette.TextureIndex, offset, sprite.Size);

            dirtyRows.Add(uv.V);


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="sprite"></param>
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

            int vertexSize = System.Runtime.InteropServices.Marshal.SizeOf<Vertex>();
            //Flush any visible changes to the GPU
            for (var row = firstRow; row <= lastRow; row++)
            {
                if (!dirtyRows.Remove(row))
                    continue;

                var rowOffset = rowStride * row;

                unsafe
                {
                    fixed(Vertex* vPtr = &vertices[0])
                    {

                    }

                }

                vertexBuffer.SetData(vertexSize*rowOffset,vertices,rowOffset,rowStride,0,SetDataOptions.None);
            }
            ((WarGame)Game).Renderer.WorldSpriteRenderer.DrawVertexBuffer(bindings, rowStride * firstRow, rowStride * (lastRow - firstRow), PrimitiveType.TriangleList, Sheet, BlendMode);
        }
        

        /// <summary>
        /// Detach event handlers on dispose in TerrainSpriteLayer
        /// 
        /// The WorldRenderer outlives the TerrainSpriteLayer and thus keeps it alive longer than expected via the event handler.
        /// We detach it to allow the GC to reclaim it
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            worldRender.PaletteInvalidated -= UpdatePaletteIndices;
            vertexBuffer.Dispose();
        }


    }
}