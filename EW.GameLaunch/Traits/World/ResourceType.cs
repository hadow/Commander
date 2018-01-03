using System;
using System.Collections.Generic;
using EW.Graphics;
namespace EW.Traits
{
    public class ResourceTypeInfo : ITraitInfo
    {
        /// <summary>
        /// Sequence image that holds the different variants.
        /// </summary>
        public readonly string Image = "resources";

        [FieldLoader.Require]
        [SequenceReference("Image")]
        public readonly string[] Sequences = { };

        /// <summary>
        /// Palette used for rendering the resource sprites
        /// </summary>
        [PaletteReference]
        public readonly string Palette = TileSet.TerrainPaletteInternalName;

        /// <summary>
        /// Resource index used in the binary map data.
        /// </summary>
        public readonly int ResourceType = 1;

        /// <summary>
        /// Credit value of a single resource unit.
        /// 每个资源单位信用值
        /// </summary>
        public readonly int ValuePerUnit = 0;

        /// <summary>
        /// Maximum number of resource units allowed in a single cell.
        /// 单个单元格中允许的最大资源数
        /// </summary>
        public readonly int MaxDensity = 10;

        /// <summary>
        /// Resource identifier used by other traits.
        /// </summary>
        /// 
        [FieldLoader.Require]
        public readonly string Type = null;

        /// <summary>
        /// Resource name used by tooltips
        /// </summary>
        /// 
        [FieldLoader.Require]
        public readonly string Name = null;

        /// <summary>
        /// Terrain type used to determine unit movement and minimap colors.
        /// </summary>
        /// 
        [FieldLoader.Require]
        public readonly string TerrainType = null;

        /// <summary>
        /// Terrain type that this resource can spawn on
        /// 资源可以产生的地形类型
        /// </summary>
        public readonly HashSet<string> AllowedTerrainTypes = new HashSet<string>();

        /// <summary>
        /// Allow resource to spawn under Mobile actors.
        /// 允许资源在移动的Actor 下生成
        /// </summary>
        public readonly bool AllowUnderActors = false;

        /// <summary>
        /// Allow resource to spawn under Buildings.
        /// 允许资源在建筑物下生成
        /// </summary>
        public readonly bool AllowUnderBuildings = false;

        /// <summary>
        /// Allow resource to spawn on ramp tiles.
        /// 允许资源在斜坡上生成
        /// </summary>
        public readonly bool AllowOnRamps = false;

        /// <summary>
        /// Harvester content pip color
        /// </summary>
        public PipType PipColor = PipType.Yellow;

        public object Create(ActorInitializer init) { return new ResourceType(this,init.World); }
    }

    public class ResourceType:IWorldLoaded
    {
        public readonly ResourceTypeInfo Info;
        public PaletteReference Palette { get; private set; }

        public readonly Dictionary<string, Sprite[]> Variants;

        public ResourceType(ResourceTypeInfo info,World world)
        {
            Info = info;
            Variants = new Dictionary<string, Sprite[]>();
            foreach(var v in info.Sequences)
            {
                var seq = world.Map.Rules.Sequences.GetSequence(info.Image, v);
                var sprites = Exts.MakeArray(seq.Length, x => seq.GetSprite(x));
                Variants.Add(v, sprites);
            }
        }

        public void WorldLoaded(World w,WorldRenderer wr)
        {
            Palette = wr.Palette(Info.Palette);
        }


    }
}