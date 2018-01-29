using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Drawing;
using EW.FileSystem;
using EW.Traits;
using EW.Primitives;
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


        public BinaryDataHeader(Stream s,EW.Framework.Point expectedSize)
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
            new MapField("ModelSequences","ModelSequenceDefinitions",required:false),
            new MapField("Weapons","WeaponDefinitions",required:false),
        };


        //Standard yaml metadata
        public string RequiresMod;
        public string Title;
        public string Author;
        public string Tileset;
        public bool LockPreview;
        public Rectangle Bounds;
        public MapVisibility Visibility = MapVisibility.Lobby;
        public string[] Categories = { "Conquest" };

        //Play and Actor yaml ,Public for access by the map importers and lint checks
        public List<MiniYamlNode> PlayerDefinitions = new List<MiniYamlNode>();
        public List<MiniYamlNode> ActorDefinitions = new List<MiniYamlNode>();

        //Custom map yaml.
        public readonly MiniYaml RuleDefinitions;
        public readonly MiniYaml SequenceDefinitions;
        public readonly MiniYaml ModelSequenceDefinitions;
        public readonly MiniYaml WeaponDefinitions;
        public readonly MiniYaml VoicDefinitions;
        public readonly MiniYaml NotificationDefinitions;
        public readonly MiniYaml MusicDefinitions;

        //Internal data
        readonly ModData modData;
        bool initializedCellProjection;
        CellLayer<PPos[]> cellProjection;
        CellLayer<List<MPos>> inverseCellProjection;
        CellLayer<short> cachedTerrainIndexes;
        CellLayer<byte> projectedHeight;


        public string Uid { get; private set; }
        /// <summary>
        /// 地图网格数据 
        /// </summary>
        public readonly MapGrid Grid;

        public IReadOnlyPackage Package { get; private set; }

        public EW.Framework.Point MapSize { get; private set; }

        public Ruleset Rules { get; private set; }
        
        public ProjectedCellRegion ProjectedCellBounds { get; private set;}

        /// <summary>
        /// The Bottom-Right of the playable area in projected world coordinates.
        /// </summary>
        /// <value>The projected bottom right.</value>
        public WPos ProjectedBottomRight { get; private set; }

        /// <summary>
        /// The Top-Lef of the playable area in projected world coordinates.
        /// </summary>
        /// <value>The projected top left.</value>
        public WPos ProjectedTopLeft { get; private set; }

        public CellLayer<TerrainTile> Tiles { get; private set; }

        public CellLayer<ResourceTile> Resources { get; private set; }

        public CellLayer<byte> Height { get; private set; }

        public CellLayer<byte> CustomTerrain { get; private set; }

        public CellRegion AllCells { get; private set; }

        public List<CPos> AllEdgeCells { get; private set; }


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

            //Layer
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

                if(header.ResourcesOffset > 0)
                {
                    s.Position = header.ResourcesOffset;
                    for(var i = 0; i < MapSize.X; i++)
                    {
                        for(var j = 0; j < MapSize.Y; j++)
                        {
                            var type = s.ReadUInt8();
                            var density = s.ReadUInt8();
                            Resources[new MPos(i, j)] = new ResourceTile(type, density);
                        }
                    }
                }

                if (header.HeightsOffset > 0)
                {
                    s.Position = header.HeightsOffset;
                    for(var i = 0; i < MapSize.X; i++)
                    {
                        for(var j = 0; j < MapSize.Y; j++)
                        {
                            Height[new MPos(i, j)] = s.ReadUInt8().Clamp((byte)0, Grid.MaximumTerrainHeight);
                        }
                    }
                }
            }

            if (Grid.MaximumTerrainHeight > 0)
            {
                Tiles.CellEntryChanged += UpdateProjection;
                Height.CellEntryChanged += UpdateProjection;
            }

            PostInit();

            Uid = ComputeUID(Package);
            
            
        }

        public bool Contains(CPos cell)
        {
            //.ToMPos() returns the same result if the X and Y coordinates are switched.X<Y is invalid in the RectangularIsometric coordinate system.
            //so we pre-filter these to avoid returning the wrong result.
            
            if (Grid.Type == MapGridT.RectangularIsometric && cell.X < cell.Y)
                return false;
            return Contains(cell.ToMPos(this));
        }

        public bool Contains(MPos uv)
        {
            // The first check ensure that the cell is within the valid map region,avoiding potential crashes in deeper code.
            //第一次检查确保单元格在有效的地图区域内，避免在更深的代码中潜在的崩溃。
            // All CellLayers have the same geometry,and CustomTerrain is convenient.
            return CustomTerrain.Contains(uv) && ContainsAllProjectedCellsCovering(uv);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        bool ContainsAllProjectedCellsCovering(MPos uv)
        {
            if (Grid.MaximumTerrainHeight == 0)
                return Contains((PPos)uv);


            //If the cell has no valid projection,then we're off the map
            var projectedCells = ProjectedCellsCovering(uv);
            if (projectedCells.Length == 0)
                return false;

            foreach (var puv in projectedCells)
                if (!Contains(puv))
                    return false;
            return true;
        }

        public bool Contains(PPos puv)
        {
            return Bounds.Contains(puv.U, puv.V);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        void UpdateProjection(CPos cell)
        {
            MPos uv;
            if(Grid.MaximumTerrainHeight == 0)
            {
                uv = cell.ToMPos(Grid.Type);
                cellProjection[cell] = new[] { (PPos)uv };
                var inverse = inverseCellProjection[uv];
                inverse.Clear();
                inverse.Add(uv);
                return;
                   
            }

            if (!initializedCellProjection)
                InitializeCellPojection();

            uv = cell.ToMPos(Grid.Type);

            // Remove old reverse projection
            // 删除旧的反向投影
            foreach (var puv in cellProjection[uv])
            {
                var temp = (MPos)puv;
                inverseCellProjection[(MPos)puv].Remove(uv);
                projectedHeight[temp] = ProjectedCellHeightInner(puv);
            }


            var projected = ProjectCellInner(uv);
            cellProjection[uv] = projected;

            foreach (var puv in projected)
            {
                var temp = (MPos)puv;
                inverseCellProjection[temp].Add(uv);

                var height = ProjectedCellHeightInner(puv);
                projectedHeight[temp] = height;

                // Propagate height up cliff faces
                while (true)
                {
                    temp = new MPos(temp.U, temp.V - 1);
                    if (!inverseCellProjection.Contains(temp) || inverseCellProjection[temp].Any())
                        break;

                    projectedHeight[temp] = height;
                }
            }

        }

        static readonly PPos[] NoProjectedCells = { };


        /// <summary>
        /// 
        /// </summary>
        /// <param name="puv"></param>
        /// <returns></returns>
        byte ProjectedCellHeightInner(PPos puv)
        {
            while (inverseCellProjection.Contains((MPos)puv))
            {
                var inverse = inverseCellProjection[(MPos)puv];
                if (inverse.Any())
                {
                    //The original games treat the top of cliffs the same way as the bottom
                    //This information isn't stored in the map data,so query the offset from the tileset.
                    var temp = inverse.MaxBy(uv => uv.V);
                    var terrain = Tiles[temp];
                    return (byte)(Height[temp] - Rules.TileSet.Templates[terrain.Type][terrain.Index].Height);
                }

                //Try the next cell down if this is a cliff face
                //如果这是一个悬崖面，尝试下一个单元格
                puv = new PPos(puv.U, puv.V + 1);
            }

            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        PPos[] ProjectCellInner(MPos uv)
        {
            var mapHeight = Height;
            if (!mapHeight.Contains(uv))
                return NoProjectedCells;

            var height = mapHeight[uv];
            if (height == 0)
                return new[] { (PPos)uv };

            //Odd-height ramps get bumped up a level to the next even height layer
            if ((height & 1) == 1)
            {
                var ti = Rules.TileSet.GetTileInfo(Tiles[uv]);
                if (ti != null && ti.RampType != 0)
                    height += 1;
            }

            var candidates = new List<PPos>();

            
            //Odd-height level tiles are equally covered by four projected tiles.
            if ((height & 1) == 1)
            {
                if ((uv.V & 1) == 1)
                    candidates.Add(new PPos(uv.U + 1, uv.V - height));
                else
                    candidates.Add(new PPos(uv.U - 1, uv.V - height));

                candidates.Add(new PPos(uv.U, uv.V - height));
                candidates.Add(new PPos(uv.U, uv.V - height + 1));
                candidates.Add(new PPos(uv.U, uv.V - height - 1));
            }
            else
                candidates.Add(new PPos(uv.U, uv.V - height));

            return candidates.Where(c => mapHeight.Contains((MPos)c)).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public PPos[] ProjectedCellsCovering(MPos uv)
        {
            if (!initializedCellProjection)
                InitializeCellPojection();

            if (!cellProjection.Contains(uv))
                return NoProjectedCells;

            return cellProjection[uv];
        }

        public PPos ProjectedCellCovering(WPos pos)
        {
            var projectedPos = pos - new WVec(0, pos.Z, pos.Z);

            return (PPos)CellContaining(projectedPos).ToMPos(Grid.Type);
        }

        /// <summary>
        /// 
        /// </summary>
        void InitializeCellPojection()
        {
            if (initializedCellProjection)
                return;

            initializedCellProjection = true;

            cellProjection = new CellLayer<PPos[]>(this);
            inverseCellProjection = new CellLayer<List<MPos>>(this);
            projectedHeight = new CellLayer<byte>(this);

            //Initialize collections
            foreach(var cell in AllCells)
            {
                var uv = cell.ToMPos(Grid.Type);
                cellProjection[uv] = new PPos[0];
                inverseCellProjection[uv] = new List<MPos>();
                    
            }

            //Initialize projections
            foreach (var cell in AllCells)
                UpdateProjection(cell);
        }

        public List<MPos> Unproject(PPos puv)
        {
            var uv = (MPos)puv;

            if (!initializedCellProjection)
                InitializeCellPojection();

            if (!inverseCellProjection.Contains(uv))
                return new List<MPos>();

            return inverseCellProjection[uv];
        }

        public byte ProjectedHeight(PPos puv)
        {
            return projectedHeight[(MPos)puv];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public CPos CellContaining(WPos pos)
        {
            if (Grid.Type == MapGridT.Rectangular)
                return new CPos(pos.X / 1024, pos.Y / 1024);

            //Convert from world position to isometric cell postion:
            //(a) Subtract ([1/2 cell],[1/2 cell]) to move the rotation center to the middle of the corner cell
            //(b) Rotate axes by -pi/4 to align the world axes with the cell axes
            //(c) Apply an offset so that the integer division by [1 cell] rounds in the right direction:
            //      (i) u is always positive,so add [1/2 cell] (which then partially cancels the -[1 cell] term from the rotation)
            //      (ii) v can be negative,so we need to be careful about rounding directions. We add [1/2 cell] * away from 0* (negative if y>x ).

            //(e) Divide by [1 cell] to bring into cell coords.
            // The world axes are rotated relative to the cell axes,so the standard cell size (1024) is increased by a factor of sqrt(2)
            var u = (pos.Y + pos.X - 724) / 1448;
            var v = (pos.Y - pos.X + (pos.Y > pos.X ? 724 : -724)) / 1448;
            return new CPos(u, v);
        }

        /// <summary>
        /// Convert from isometric cell position to world position;
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public WPos CenterOfCell(CPos cell)
        {
            if (Grid.Type == MapGridT.Rectangular)
                return new WPos(1024 * cell.X + 512, 1024 * cell.Y + 512, 0);

            //Convert from isometric cell position (x,y) to world position (u,v):
            //(a) Consider the relationships:
            //  - Center of origin cell is (512,512)
            //  - +x adds (512,512) to world pos
            //  - +y adds (-512,512) to world pos
            //(b) Therefore:
            // - ax+by adds (a-b)*512 + 512 to u
            // - ax+by adds (a+b)*512 + 512 to v
            //(c) u,v coordinates run diagonally to the cell axes, and we define 1024 as the length projected onto the primary cell axis
            //u，v坐标以单元轴对角线方式运行，我们将1024定义为投影到主单元轴上的长度
            // - 512*sqrt(2) = 724;
            var z = Height.Contains(cell) ? 724 * Height[cell] : 0;
            return new WPos(724 * (cell.X - cell.Y + 1), 724 * (cell.X + cell.Y + 1), z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="subCell"></param>
        /// <returns></returns>
        public WPos CenterOfSubCell(CPos cell,SubCell subCell)
        {
            var index = (int)subCell;
            if (index >= 0 && index <= Grid.SubCellOffsets.Length)
                return CenterOfCell(cell) + Grid.SubCellOffsets[index];
            return CenterOfCell(cell);
        }

        /// <summary>
        /// 
        /// </summary>
        void PostInit()
        {
            try
            {
                Rules = Ruleset.Load(modData, this, Tileset, RuleDefinitions, WeaponDefinitions, 
                    VoicDefinitions, NotificationDefinitions, MusicDefinitions, SequenceDefinitions,
                    ModelSequenceDefinitions);

            }
            catch (Exception e)
            {

                Rules = Ruleset.LoadDefaultsForTileSet(modData, Tileset);
            }

            Rules.Sequences.PreLoad();

            var tl = new MPos(0, 0).ToCPos(this);
            var br = new MPos(MapSize.X - 1, MapSize.Y - 1).ToCPos(this);

            AllCells = new CellRegion(Grid.Type, tl, br);

            var btl = new PPos(Bounds.Left, Bounds.Top);
            var bbr = new PPos(Bounds.Right - 1, Bounds.Bottom - 1);

            SetBounds(btl, bbr);

            CustomTerrain = new CellLayer<byte>(this);
            foreach(var uv in AllCells.MapCoords)
            {
                CustomTerrain[uv] = byte.MaxValue;
            }
            AllEdgeCells = UpdateEdgeCells();
            

        }

        List<CPos> UpdateEdgeCells()
        {
            var edgeCells = new List<CPos>();
            var unProjected = new List<MPos>();
            var bottom = Bounds.Bottom - 1;

            for(var u = Bounds.Left; u < Bounds.Right; u++)
            {

            }

            return edgeCells;
        }


        public CPos Clamp(CPos cell)
        {
            return default(CPos);
        }


        public MPos Clamp(MPos uv)
        {
            return default(MPos);
        }

        

        public PPos Clamp(PPos puv)
        {
            var bounds = new Rectangle(Bounds.X, Bounds.Y, Bounds.Width - 1, Bounds.Height - 1);
            return puv.Clamp(bounds);
        }


        /// <summary>
        /// 设定地图边界
        /// </summary>
        /// <param name="tl"></param>
        /// <param name="br"></param>
        public void SetBounds(PPos tl,PPos br)
        {
            //The tl and br coordinates are inclusive,but the Rectangle is exclusive.
            //Pad the right and bootom edges to match.
            Bounds = Rectangle.FromLTRB(tl.U, tl.V, br.U + 1, br.V + 1);
            //避免不必要的转换，直接计算地图屏幕投射坐标的世界单位
            var wtop = tl.V * 1024;
            var wbottom = (br.V + 1) * 1024;
            if(Grid.Type == MapGridT.RectangularIsometric)
            {
                //wtop /= 2;
                //wbottom /= 2;
                ProjectedTopLeft = new WPos(tl.U * 1448, tl.V * 724, 0);
                ProjectedBottomRight = new WPos(br.U * 1448 - 1, (br.V + 1) * 724 - 1, 0);
            }
            else
            {

                ProjectedTopLeft = new WPos(tl.U * 1024, wtop, 0);
                ProjectedBottomRight = new WPos(br.U * 1024 - 1, wbottom - 1, 0);
            }

            ProjectedCellBounds = new ProjectedCellRegion(this, tl, br);
        }

        /// <summary>
        /// 在地图中缓存tileset　查找，所以GetTerrainIndex 和 GetTerrainInfo 不需要每次重复loop,
        /// 查找占了所花费时间的50~60%，占用CPU总量的1.3%,这是一个很小但可衡量的结果
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public TerrainTypeInfo GetTerrainInfo(CPos cell)
        {
            return Rules.TileSet[GetTerrainIndex(cell)];
        }

        /// <summary>
        /// 获取某一单元格所代表的地形索引
        /// Transparently cache results of GetTerrainIndex in Map.
        /// 
        /// This method performs an expensive calculation and is called often during pathfinding.
        /// We create a cache of the terrain indicies for the map to vastly reduce the cost.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public byte GetTerrainIndex(CPos cell)
        {
            const short InvalidCachedTerrainIndex = -1;
            //Lazily initialize a cache for terrain indexes;懒初始化地形索引缓存
            if(cachedTerrainIndexes == null)
            {
                cachedTerrainIndexes = new CellLayer<short>(this);
                cachedTerrainIndexes.Clear(InvalidCachedTerrainIndex);


                //Invalidate the entry for a cell if anything could cause the terrain index to change;
                Action<CPos> invalidateTerrainIndex = c => cachedTerrainIndexes[c] = InvalidCachedTerrainIndex;
                CustomTerrain.CellEntryChanged += invalidateTerrainIndex;
                Tiles.CellEntryChanged += invalidateTerrainIndex;
            }

            var uv = cell.ToMPos(this);

            var terrainIndex = cachedTerrainIndexes[uv];

            //PERF:Cache terrain indexes per cell on demand.//按需求对每个单元格缓存地形索引
            if(terrainIndex == InvalidCachedTerrainIndex)
            {
                var custom = CustomTerrain[uv];
                terrainIndex = cachedTerrainIndexes[uv] = custom != byte.MaxValue ? custom : Rules.TileSet.GetTerrainIndex(Tiles[uv]);
            }

            return (byte)terrainIndex;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Stream Open(string filename)
        {
            if(!filename.Contains("|") && Package.Contains(filename))
            {
                return Package.GetStream(filename);
            }

            return modData.DefaultFileSystem.Open(filename);
        }

        public bool TryGetPackageContaining(string path,out IReadOnlyPackage package,out string filename)
        {
            return modData.DefaultFileSystem.TryGetPackageContaining(path, out package, out filename);
        }

        public bool TryOpen(string filename,out Stream s)
        {
            if(!filename.Contains("|"))
            {
                s = Package.GetStream(filename);
                if (s != null)
                    return true;
            }

            return modData.DefaultFileSystem.TryOpen(filename, out s);
        }

        public bool Exists(string filename)
        {
            if (!filename.Contains("|") && Package.Contains(filename))
                return true;
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

            //using(var ms = new MemoryStream())
            //{
            //    foreach(var filename in contents)
            //    {
            //        if(filename.EndsWith(".yaml") || filename.EndsWith(".bin") || filename.EndsWith(".lua"))
            //        {
            //            using (var s = package.GetStream(filename))
            //                s.CopyTo(ms);
            //        }
            //    }

            //    ms.Seek(0, SeekOrigin.Begin);
            //    return CryptoUtil.SHA1Hash(ms);
            //}
            var streams = new List<Stream>();

            try
            {
                foreach(var filename in contents){

                    if (filename.EndsWith(".yaml") || filename.EndsWith(".bin") || filename.EndsWith(".lua"))
                        streams.Add(package.GetStream(filename));

                }

                //Take the SHA1
                if (streams.Count == 0)
                    return CryptoUtil.SHA1Hash(new byte[0]);

                var merged = streams[0];
                for (var i = 1; i < streams.Count; i++)
                {
                    merged = new MergedStream(merged, streams[i]);

                }

                return CryptoUtil.SHA1Hash(merged);
            }
            finally{
                foreach(var stream in streams)
                    stream.Dispose();
            }


        }

        /// <summary>
        /// 在地形之上的距离
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public WDist DistanceAboveTerrain(WPos pos)
        {
            var cell = CellContaining(pos);
            var delta = pos - CenterOfCell(cell);
            return new WDist(delta.Z);
        }

        public int FacingBetween(CPos cell,CPos towards,int fallbackfacing)
        {
            var delta = CenterOfCell(towards) - CenterOfCell(cell);
            if (delta.HorizontalLengthSquared == 0)
                return fallbackfacing;
            return delta.Yaw.Facing;
        }


        public IEnumerable<CPos> FindTilesInCircle(CPos center,int maxRange,bool allowOutsideBounds = false){

            return FindTilesInAnnulus(center, 0, maxRange, allowOutsideBounds);
        }


        /// <summary>
        // Both ranges are inclusive because everything that calls it is designed for maxRange being inclusive:
        // it rounds the actual distance up to the next integer so that this call
        // will return any cells that intersect with the requested range circle.
        // The returned positions are sorted by distance from the center.
        /// 这两个范围都是包含的，因为调用它的所有内容都是为包含的maxRange设计的：
        /// 它将实际距离四舍五入到下一个整数，
        /// 以便此调用将返回与请求的范围圆相交的任何单元格。返回的位置按距离中心的距离排序。

        /// </summary>
        /// <returns>The tiles in annulus.</returns>
        /// <param name="center">Center.</param>
        /// <param name="minRange">Minimum range.</param>
        /// <param name="maxRange">Max range.</param>
        /// <param name="allowOutsideBounds">If set to <c>true</c> allow outside bounds.</param>

        public IEnumerable<CPos> FindTilesInAnnulus(CPos center,int minRange,int maxRange,bool allowOutsideBounds = false){


            if (maxRange < minRange)
                throw new ArgumentOutOfRangeException("maxRange", "Maximum range is less than the minimum range.");

            if (maxRange >= Grid.TilesByDistance.Length)
                throw new ArgumentOutOfRangeException("maxRange", "The requested range ({0}) cannot exceed the value of MaximumTileSearchRange ({1})  ".F(maxRange, Grid.MaximumTileSearchRange));

            Func<CPos, bool> valid = Contains;

            if (allowOutsideBounds)
                valid = Tiles.Contains;

            for (var i = minRange; i <= maxRange;i++){
                foreach(var offset in Grid.TilesByDistance[i]){
                    var t = offset + center;
                    if (valid(t))
                        yield return t;
                }
            }
        }


    }
}