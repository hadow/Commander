using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
using EW.FileSystem;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class TerrainTileInfo
    {
        public readonly byte TerrainT = byte.MaxValue;
        public readonly byte Height;
        public readonly byte RampT;//斜坡
        public readonly Color LeftColor;
        public readonly Color RightColor;

        public readonly float ZOffset = 0.0f;
        public readonly float ZRamp = 1.0f;

    }

    /// <summary>
    /// 地形类型信息
    /// </summary>
    public class TerrainTypeInfo
    {
        static readonly TerrainTypeInfo Default = new TerrainTypeInfo();

        public readonly string Type;
        public readonly HashSet<string> TargetTypes = new HashSet<string>();
        public readonly HashSet<string> AcceptsSumudgeType = new HashSet<string>();
        public readonly Color Color;
        public readonly bool RestrictPlayerColor = false;
        public readonly string CustomCursor;

        TerrainTypeInfo() { }

        public TerrainTypeInfo(MiniYaml my)
        {
            FieldLoader.Load(this, my);
        }
    }

    public class TerrainTemplateInfo
    {
        public readonly ushort Id;
        public readonly string[] Images;
        public readonly int[] Frames;
        public readonly Vector2 Size;
        public readonly bool PickAny;
        public readonly string Category;
        public readonly string Palette;

        readonly TerrainTileInfo[] tileInfo;

        public TerrainTileInfo this[int index]
        {
            get { return tileInfo[index]; }
        }

        public int TileCount
        {
            get { return tileInfo.Length; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TileSet
    {
        public const string TerrainPaletteInternalName = "terrain";

        public readonly string Name;

        public readonly string Id;

        public readonly int SheetSize = 512;

        public readonly string Palette;

        public readonly string PlayerPalette;

        public TileSet(IReadOnlyFileSystem fileSystem,string filePath)
        {
            var yaml = MiniYaml.DictFromStream(fileSystem.Open(filePath), filePath);
        }
    }
}