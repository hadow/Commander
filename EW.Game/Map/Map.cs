using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EW.FileSystem;
using EW.Xna.Platforms;
namespace EW
{
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
    }
    /// <summary>
    /// 
    /// </summary>
    public class Map:IReadOnlyFileSystem
    {
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
        public Rectangle Bounds;

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


        public CellLayer<byte> Height { get; private set;}

        public ProjectedCellRegion ProjectedCellBounds { get; private set;}
        
        public Map(ModData modData,IReadOnlyPackage package)
        {
            this.modData = modData;
            Package = package;

            if (!Package.Contains("map.yaml") || !Package.Contains("map.bin"))
                throw new InvalidDataException("Not a valid map\n File:{0}".F(package.Name));

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

    }
}