using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Drawing;
using EW.FileSystem;
using EW.Xna.Platforms;
namespace EW
{
    /// <summary>
    /// 头文件格式
    /// </summary>
    struct BinaryDataHeader
    {
        public readonly byte Format;
        public readonly uint TilesOffset;
        public readonly uint HeightsOffset;
        public readonly uint ResourcesOffset;


        public BinaryDataHeader(Stream s,Vector2 expectedSize)
        {
            Format = s.ReadUInt8();
            var width = s.ReadUInt16();
            var height = s.ReadUInt16();
            if(width!= expectedSize.X || height != expectedSize.Y)
            {
                throw new InvalidDataException("Invalid tile data");
            }

            if (Format == 1)
            {
                TilesOffset = 5;
                HeightsOffset = 0;
                ResourcesOffset = (uint)(3 * width * height + 5);
            }
            else if (Format == 2)
            {
                TilesOffset = s.ReadUInt32();
                HeightsOffset = s.ReadUInt32();
                ResourcesOffset = s.ReadUInt32();

            }
            else
                throw new InvalidDataException("Unknown binary map format '{0}'".F(Format));
        }
    }


    public enum MapVisibility
    {
        Lobby = 1,
        Shellmap = 2,
        MissionSelector = 4,
    }
    class MapField
    {
        enum Type
        {
            Noraml,
            NodeList,
            MiniYaml,
        }

        readonly FieldInfo field;
        readonly PropertyInfo property;
        readonly Type type;

        readonly string key;
        readonly string fieldName;
        readonly bool required;
        readonly string ignoreIfValue;
            
        public MapField(string key,string fieldName = null,bool required = true,string ignoreIfValue = null)
        {
            this.key = key;
            this.fieldName = fieldName ?? key;
            this.required = required;
            this.ignoreIfValue = ignoreIfValue;

            field = typeof(Map).GetField(this.fieldName);
            property = typeof(Map).GetProperty(this.fieldName);

            if (field == null && property == null)
                throw new InvalidOperationException("Map does not have a field/property {0}".F(fieldName));

            var t = field != null ? field.FieldType : property.PropertyType;

            type = t == typeof(List<MiniYamlNode>) ? Type.NodeList : t == typeof(MiniYaml) ? Type.MiniYaml : Type.Noraml;

                

        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="map"></param>
        /// <param name="nodes"></param>
        public void Deserialize(Map map,List<MiniYamlNode> nodes)
        {
            var node = nodes.FirstOrDefault(n => n.Key == key);
            if(node == null)
            {
                if (required)
                    throw new YamlException("Required field '{0}' not found in map.yaml".F(key));
                return;
            }

            if (field != null)
            {
                if (type == Type.NodeList)
                    field.SetValue(map, node.Value.Nodes);
                else if (type == Type.MiniYaml)
                    field.SetValue(map, node.Value);
                else
                    FieldLoader.LoadField(map, fieldName, node.Value.Value);
            }

            if(property != null)
            {
                if (type == Type.NodeList)
                    property.SetValue(map, node.Value.Nodes, null);
                else if (type == Type.MiniYaml)
                    property.SetValue(map, node.Value, null);
                else
                    FieldLoader.LoadField(map, fieldName, node.Value.Value);
            }

                
        }


    }
    /// <summary>
    /// 
    /// </summary>
    public class Map:IReadOnlyFileSystem
    {

        //Format versions
        public int MapFormat { get; private set; }
        public readonly byte TileFormat = 2;

        public const int SupportedMapFormat = 11;

        static readonly MapField[] YamlFields =
        {
            new MapField("MapFormat"),
            new MapField("RequiresMod"),
            new MapField("Title"),
            new MapField("Author"),
            new MapField("Tileset"),
            new MapField("MapSize"),
            new MapField("Bounds"),
            new MapField("Visibility"),
            new MapField("Categories"),
            new MapField("LockPreview",required:false,ignoreIfValue:"False"),
            new MapField("Players","PlayerDefinitions"),
            new MapField("Actors","ActorDefinitions"),
            new MapField("Rules","RuleDefinitions",required:false),
            new MapField("Sequences","SequenceDefinitions",required:false),
            new MapField("VoxelSequences","VoxelSequenceDefinitions",required:false),
            new MapField("Weapons","WeaponDefinitions",required:false),
        };


        //Standard yaml metadata
        public string RequiresMod;
        public string Title;
        public string Author;
        public string Tileset;
        public bool LockPreview;
        public EW.Xna.Platforms.Rectangle Bounds;

        public List<MiniYamlNode> PlayerDefinitions = new List<MiniYamlNode>();
        public List<MiniYamlNode> ActorDefinitions = new List<MiniYamlNode>();
        
        public readonly MiniYaml RuleDefinitions;
        public readonly MiniYaml SequenceDefinitions;
        public readonly MiniYaml VoxelSequenceDefinitions;
        public readonly MiniYaml WeaponDefinitions;
        public readonly MiniYaml VoicDefinitions;
        public readonly MiniYaml NotificationDefinitions;
        public readonly MiniYaml MusicDefinitions;

        readonly ModData modData;

        /// <summary>
        /// 地图网格数据 
        /// </summary>
        public readonly MapGrid Grid;

        public IReadOnlyPackage Package { get; private set; }

        public Vector2 MapSize { get; private set; }

        public Ruleset Rules { get; private set; }
        
        public ProjectedCellRegion ProjectedCellBounds { get; private set;}
        

        public CellLayer<TerrainTile> Tiles { get; private set; }

        public CellLayer<ResourceTile> Resources { get; private set; }

        public CellLayer<byte> Height { get; private set; }

        public CellLayer<byte> CustomTerrain { get; private set; }
        public Map(ModData modData,IReadOnlyPackage package)
        {
            this.modData = modData;
            Package = package;

            if (!Package.Contains("map.yaml") || !Package.Contains("map.bin"))
                throw new InvalidDataException("Not a valid map\n File:{0}".F(package.Name));

            var yaml = new MiniYaml(null, MiniYaml.FromStream(Package.GetStream("map.yaml"), package.Name));
            foreach(var field in YamlFields)
            {
                field.Deserialize(this, yaml.Nodes);
            }

            if(MapFormat != SupportedMapFormat)
            {
                throw new InvalidDataException("Map format {0} is not supported. \n File:{1}".F(MapFormat, package.Name));
            }
            PlayerDefinitions = MiniYaml.NodesOrEmpty(yaml, "Players");
            ActorDefinitions = MiniYaml.NodesOrEmpty(yaml, "Actors");

            Grid = modData.Manifest.Get<MapGrid>();

            var size = new Size((int)MapSize.X, (int)MapSize.Y);
            Tiles = new CellLayer<TerrainTile>(Grid.Type, size);
            Resources = new CellLayer<ResourceTile>(Grid.Type, size);
            Height = new CellLayer<byte>(Grid.Type, size);
            

            using(var s = Package.GetStream("map.bin"))
            {
                var header = new BinaryDataHeader(s, MapSize);

                if (header.TilesOffset > 0)
                {
                    s.Position = header.TilesOffset;
                    for(var i = 0; i < MapSize.X; i++)
                    {
                        for(var j = 0; j < MapSize.Y; j++)
                        {
                            var tile = s.ReadUInt16();
                            var index = s.ReadUInt8();
                            if (index == byte.MaxValue)
                                index = (byte)(i % 4 + (j % 4) * 4);

                            Tiles[new MPos(i, j)] = new TerrainTile(tile, index);
                        }
                    }
                }
            }
        }


        void PostInit()
        {
            try
            {
                Rules = Ruleset.Load(modData, this, Tileset, RuleDefinitions, WeaponDefinitions, VoicDefinitions, NotificationDefinitions, MusicDefinitions, SequenceDefinitions);

            }
            catch ()
            {

            }
           
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Stream Open(string filename)
        {
            return modData.DefaultFileSystem.Open(filename);
        }

        public bool Exists(string filename)
        {
            return modData.DefaultFileSystem.Exists(filename);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static string ComputeUID(IReadOnlyPackage package)
        {
            var requiredFiles = new[] { "map.yaml", "map.bin" };
            var contents = package.Contents.ToList();

            foreach(var required in requiredFiles)
            {
                if (!contents.Contains(required))
                    throw new FileNotFoundException("Required file {0} not present in this map".F(required));
            }

            using(var ms = new MemoryStream())
            {
                foreach(var filename in contents)
                {
                    if(filename.EndsWith(".yaml") || filename.EndsWith(".bin") || filename.EndsWith(".lua"))
                    {
                        using (var s = package.GetStream(filename))
                            s.CopyTo(ms);
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);
                return CryptoUtil.SHA1Hash(ms);
            }



        }

    }
}