using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using EW.Framework;
using EW.FileSystem;
using System.Drawing;
namespace EW
{
    /// <summary>
    /// 地形切片信息
    /// </summary>
    public class TerrainTileInfo
    {
        [FieldLoader.Ignore]
        public readonly byte TerrainT = byte.MaxValue;
        public readonly byte Height;
        public readonly byte RampType;//斜坡

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
        public readonly HashSet<string> AcceptsSmudgeType = new HashSet<string>();
        public readonly Color Color;
        public readonly bool RestrictPlayerColor = false;//限定player 颜色
        public readonly string CustomCursor;

        TerrainTypeInfo() { }

        public TerrainTypeInfo(MiniYaml my)
        {
            FieldLoader.Load(this, my);
        }
    }

    /// <summary>
    /// 地形模板信息
    /// </summary>
    public class TerrainTemplateInfo
    {
        public readonly ushort Id;

        public readonly string[] Images;

        public readonly int[] Frames;

        public readonly EW.Framework.Point Size;

        public readonly bool PickAny;

        public readonly string Category;

        public readonly string Palette;

        readonly TerrainTileInfo[] tileInfo;


        public TerrainTemplateInfo(TileSet tileSet,MiniYaml my)
        {
            FieldLoader.Load(this, my);

            var nodes = my.ToDictionary()["Tiles"].Nodes;

            if (!PickAny)
            {
                tileInfo = new TerrainTileInfo[Size.X * Size.Y];
                foreach(var node in nodes)
                {
                    int key;
                    if (!int.TryParse(node.Key, out key) || key < 0 || key >= tileInfo.Length)
                        throw new InvalidDataException("Invalid tile key '{0}' on template '{1}' of tileset '{2}'.".F(node.Key,Id,tileSet.Id));

                    tileInfo[key] = LoadTileInfo(tileSet, node.Value);
                }
            }
            else
            {
                tileInfo = new TerrainTileInfo[nodes.Count];

                var i = 0;
                foreach(var node in nodes)
                {
                    int key;
                    if(!int.TryParse(node.Key,out key) || key != i++)
                        throw new InvalidDataException("Invalid tile key '{0}' on template '{1}' of tileset '{2}'.".F(node.Key, Id, tileSet.Id));

                    tileInfo[key] = LoadTileInfo(tileSet, node.Value);
                }
            }
        }

        public bool Contains(int index)
        {
            return index >= 0 && index < tileInfo.Length;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileSet"></param>
        /// <param name="my"></param>
        /// <returns></returns>
        static TerrainTileInfo LoadTileInfo(TileSet tileSet,MiniYaml my)
        {
            var tile = new TerrainTileInfo();
            FieldLoader.Load(tile, my);

            //Terrain type must be converted from a string to an index.
            tile.GetType().GetField("TerrainT").SetValue(tile, tileSet.GetTerrainIndex(my.Value));

            //Fall back to the terrain-type color if necessary
            var overrideColor = tileSet.TerrainInfo[tile.TerrainT].Color;
            if (tile.LeftColor == default(Color))
                tile.GetType().GetField("LeftColor").SetValue(tile, overrideColor);

            if (tile.RightColor == default(Color))
                tile.GetType().GetField("RightColor").SetValue(tile, overrideColor);

            return tile;
        }

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
    /// 地形切片集
    /// </summary>
    public class TileSet
    {
        public const string TerrainPaletteInternalName = "terrain";

        public readonly string Name;

        public readonly string Id;

        public readonly int SheetSize = 512;

        public readonly string Palette;

        public readonly string PlayerPalette;

        public readonly Color[] HeightDebugColors = new[] { Color.Red };

        public readonly bool EnableDepth = false;

        public readonly bool IgnoreTileSpriteOffsets;

        [FieldLoader.Ignore]
        public readonly TerrainTypeInfo[] TerrainInfo;
        //Impassable(不可逾越)
        //Road,
        //Rail(铁轨),
        //Bridge(桥),
        //Water(水面),
        //DirtRoad(泥土路),
        //rough(粗糙的),
        //cliff(悬崖),
        //Veins(山脉),
        //rock (岩石),
        //subterranean(地下)
        //Jumpjet
        readonly Dictionary<string, byte> terrainIndexByType = new Dictionary<string, byte>();

        public readonly IReadOnlyDictionary<ushort, TerrainTemplateInfo> Templates;

        readonly byte defaultWalkableTerrainIndex;

        public TileSet(IReadOnlyFileSystem fileSystem,string filePath)
        {
            var yaml = MiniYaml.DictFromStream(fileSystem.Open(filePath), filePath);

            //General Info
            FieldLoader.Load(this, yaml["General"]);

            //Terrain Types
            TerrainInfo = yaml["Terrain"].ToDictionary().Values.Select(y => new TerrainTypeInfo(y)).OrderBy(tt => tt.Type).ToArray();

            if (TerrainInfo.Length >= byte.MaxValue)
                throw new InvalidOperationException("Too many terrain types.");

            for(byte i = 0; i < TerrainInfo.Length; i++)
            {
                var tt = TerrainInfo[i].Type;
                if (terrainIndexByType.ContainsKey(tt))
                    throw new InvalidOperationException("Duplicate terrain type '{0}' in '{1}'".F(tt, filePath));

                terrainIndexByType.Add(tt, i);
            }

            defaultWalkableTerrainIndex = GetTerrainIndex("Clear");

            //Templates
            Templates = yaml["Templates"].ToDictionary().Values.Select(y => new TerrainTemplateInfo(this,y)).ToDictionary(t => t.Id).AsReadOnly();

        }

        public TerrainTypeInfo this[byte index]
        {
            get { return TerrainInfo[index]; }
        }

        /// <summary>
        /// 根据地形切片获取索引
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public byte GetTerrainIndex(TerrainTile r)
        {
            TerrainTemplateInfo tpl;
            if (!Templates.TryGetValue(r.Type, out tpl))
                return defaultWalkableTerrainIndex;

            if (tpl.Contains(r.Index))
            {
                var tile = tpl[r.Index];
                if(tile != null && tile.TerrainT != byte.MaxValue)
                {
                    return tile.TerrainT;
                }
            }
            return defaultWalkableTerrainIndex;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public byte GetTerrainIndex(string type)
        {
            byte index;
            if (terrainIndexByType.TryGetValue(type, out index))
                return index;

            throw new InvalidDataException("Tileset '{0}' lacks terrain type '{1}'".F(Id, type));
        }

        public TerrainTileInfo GetTileInfo(TerrainTile r)
        {
            TerrainTemplateInfo tpl;
            if(!Templates.TryGetValue(r.Type,out tpl))
            {
                return null;
            }
            return tpl.Contains(r.Index) ? tpl[r.Index] : null;
        }

        public bool TryGetTerrainIndex(string type,out byte index)
        {
            return terrainIndexByType.TryGetValue(type, out index);
        }


    }
}