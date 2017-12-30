using System;
using System.IO;
using System.Collections.Generic;
using EW.OpenGLES.Graphics;
using EW.OpenGLES;
namespace EW.Graphics
{
    /// <summary>
    /// 地形图层
    /// </summary>
    public sealed class TerrainSpriteLayer:IDisposable
    {
        /// <summary>
        /// 限定边界
        /// </summary>
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
        readonly IVertexBuffer<Vertex> vertexBuffer;
        
        readonly HashSet<int> dirtyRows = new HashSet<int>();

        readonly int rowStride;

        readonly Vertex[] vertices;

        public TerrainSpriteLayer(World world,WorldRenderer wr,Sheet sheet,BlendMode blendMode,PaletteReference palette,bool restrictToBounds)
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


            vertexBuffer = WarGame.Renderer.Device.CreateVertexBuffer(vertices.Length);

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
                vertices[i] = new Vertex(v.X,v.Y,v.Z,v.S,v.T,v.U,v.V, palette.TextureIndex, v.C);
            }


            for(var row = 0; row < map.MapSize.Y; row++)
            {
                dirtyRows.Add(row);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="sprite"></param>
        /// <param name="pos"></param>
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

            //The vertex buffer does not have geometry for cells outside the map
            //顶点缓冲区没有地图外的单元格的几何图形
            if (!map.Tiles.Contains(uv))
                return;

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
            var xyz = sprite == null ? Vector3.Zero : 
                worldRender.Screen3DPosition(map.CenterOfCell(cell)) + sprite.Offset -  sprite.Size*0.5f;

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

            WarGame.Renderer.Flush();
            //int vertexSize = System.Runtime.InteropServices.Marshal.SizeOf<Vertex>();
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
                        vertexBuffer.SetData((IntPtr)(vPtr+rowOffset),rowOffset,rowStride);
                    }

                }

                
            }
            WarGame.Renderer.WorldSpriteRenderer.DrawVertexBuffer(vertexBuffer,
                                                                    rowStride * firstRow,
                                                                        rowStride * (lastRow - firstRow),
                                                                        PrimitiveType.TriangleList,
                                                                        Sheet,
                                                                        BlendMode);

            WarGame.Renderer.Flush();
        }
        

        /// <summary>
        /// Detach event handlers on dispose in TerrainSpriteLayer
        /// 
        /// The WorldRenderer outlives the TerrainSpriteLayer and thus keeps it alive longer than expected via the event handler.
        /// We detach it to allow the GC to reclaim it
        /// </summary>
        public void Dispose()
        {
            worldRender.PaletteInvalidated -= UpdatePaletteIndices;
            vertexBuffer.Dispose();
        }


    }
}