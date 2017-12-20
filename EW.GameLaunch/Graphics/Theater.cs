using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using EW.Support;
using EW.OpenGLES;
namespace EW.Graphics
{

    class TheaterTemplate
    {
        public readonly Sprite[] Sprites;
        public readonly int Stride;
        public readonly int Variants;

        public TheaterTemplate(Sprite[] sprites,int stride,int variants)
        {
            Sprites = sprites;
            Stride = stride;
            Variants = variants;
        }
    }
    /// <summary>
    /// 剧场
    /// </summary>
    public sealed class Theater
    {
        readonly Dictionary<ushort, TheaterTemplate> templates = new Dictionary<ushort, TheaterTemplate>();
        readonly SheetBuilder sheetBuilder;

        TileSet tileset;

        readonly MersenneTwister random;

        readonly Sprite missingTile;

        public Sheet Sheet { get { return sheetBuilder.Current; } }
        

        public Theater(TileSet tileset)
        {
            this.tileset = tileset;

            var allocated = false;

            Func<Sheet> allocate = () =>
            {
                if (allocated)
                    throw new SheetOverflowException("Terrain sheet overflow.Try increasing the tileset SheetSize parameter.");
                allocated = true;
                return new Sheet(SheetT.Indexed, new Size(tileset.SheetSize, tileset.SheetSize));
            };

            sheetBuilder = new SheetBuilder(SheetT.Indexed, allocate);
            random = new MersenneTwister();

            var frameCache = new FrameCache(WarGame.ModData.DefaultFileSystem, WarGame.ModData.SpriteLoaders);

            foreach(var t in tileset.Templates)
            {
                var variants = new List<Sprite[]>();

                foreach(var i in t.Value.Images)
                {
                    var allFrames = frameCache[i];
                    var frameCount = tileset.EnableDepth ? allFrames.Length / 2 : allFrames.Length;
                    var indices = t.Value.Frames != null ? t.Value.Frames : Enumerable.Range(0, frameCount);
                    variants.Add(indices.Select(j => {

                        var f = allFrames[j];
                        var tile = t.Value.Contains(j)?t.Value[j]:null;

                        var zOffset = tile != null ? -tile.ZOffset : 0;

                        var zRamp = tile != null ? -tile.ZRamp : 1f;

                        var offset = new Vector3(f.Offset, zOffset);

                        var s = sheetBuilder.Allocate(f.Size, zRamp, offset);

                        Util.FastCopyIntoChannel(s, f.Data);

                        if (tileset.EnableDepth)
                        {
                            var ss = sheetBuilder.Allocate(f.Size, zRamp, offset);
                            Util.FastCopyIntoChannel(ss, allFrames[j + frameCount].Data);

                            //s and ss are guaranteed to use the same sheet because of the custom terrain sheet allocation.
                            s = new SpriteWithSecondaryData(s, ss.Bounds, ss.Channel);
                        }
                        return s;
                    }).ToArray());

                    var allSprites = variants.SelectMany(s => s);

                    if (tileset.IgnoreTileSpriteOffsets)
                        allSprites = allSprites.Select(s => new Sprite(s.Sheet, s.Bounds, s.ZRamp, new Vector3(Vector2.Zero, s.Offset.Z), s.Channel, s.BlendMode));

                    templates.Add(t.Value.Id, new TheaterTemplate(allSprites.ToArray(), variants.First().Count(), t.Value.Images.Length));
                }
            }

            missingTile = sheetBuilder.Add(new byte[1], new Size(1, 1));

            Sheet.ReleaseBuffer();
        }

        /// <summary>
        /// 地形切片精灵
        /// </summary>
        /// <param name="r"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        public Sprite TileSprite(TerrainTile r,int? variant = null)
        {
            TheaterTemplate template;
            if (!templates.TryGetValue(r.Type, out template))
                return missingTile;

            if (r.Index >= template.Stride)
                return missingTile;

            var start = template.Variants > 1 ? variant.HasValue ? variant.Value : random.Next(template.Variants) : 0;
            return template.Sprites[start * template.Stride + r.Index];
        }

        //public void Dispose()
        //{
        //    sheetBuilder.Dispose();
        //}

        public void Dispose()
        {
            sheetBuilder.Dispose();
        }
    }
}