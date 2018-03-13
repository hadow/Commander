using System;
using System.IO;
using System.Linq;
using System.Drawing;
using Android.Graphics;
using System.Collections.Generic;
using EW.FileSystem;
using EW.Graphics;
using EW.Primitives;
namespace EW
{
    public enum MapClassification
    {
        Unknown,
        System,
        User,
        Remote,
    }

    public enum MapStatus
    {
        Available,
        Unavailable,
        Searching,
        DownloadAvailable,
        Downloading,
        DownloadError,
    }

    /// <summary>
    /// 
    /// </summary>
    public class MapPreview:IReadOnlyFileSystem,IDisposable
    {
        class InnerData
        {
            public string Title;
            public string[] Categories;
            public string Author;
            public string TileSet;
            public MapPlayers Players;
            public int PlayerCount;
            public CPos[] SpawnPoints;  //出生点
            public MapGridT GridT;
            public Rectangle Bounds;
            public Bitmap Preview;
            public MapStatus Status;
            public MapVisibility Visibility;
            public MapClassification Class;

            Lazy<Ruleset> rules;
            public Ruleset Rules { get { return rules != null ? rules.Value : null; } }

            
            public bool InvalidCustomRules { get; private set; }
            public bool RulesLoaded { get; private set; }

            public bool DefinesUnsafeCustomRules { get; private set; }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="modData"></param>
            /// <param name="generator"></param>
            public void SetRulesetGenerator(ModData modData,Func<Pair<Ruleset,bool>> generator)
            {
                InvalidCustomRules = false;
                RulesLoaded = false;
                DefinesUnsafeCustomRules = false;

                //可能会有多个线程同时访问，依赖Lazy<T>保证线程安全，
                rules = Exts.Lazy(() => {

                    if (generator == null)
                        return Ruleset.LoadDefaultsForTileSet(modData, TileSet);

                    try
                    {
                        var ret = generator();
                        DefinesUnsafeCustomRules = ret.Second;
                        return ret.First;
                    }
                    catch(Exception e)
                    {
                        InvalidCustomRules = true;
                        return Ruleset.LoadDefaultsForTileSet(modData, TileSet);
                    }
                    finally
                    {
                        RulesLoaded = true;
                    }
                });
            }

            public InnerData Clone()
            {
                return (InnerData)MemberwiseClone();
            }

        }

        volatile InnerData innerData;//可以由多个同时执行的线程修改

        public string Title { get { return innerData.Title; } }

        public string[] Categories { get { return innerData.Categories; } }

        public string Author { get { return innerData.Author; } }

        public string TileSet { get { return innerData.TileSet; } }

        public MapPlayers Players { get { return innerData.Players; } }

        public int PlayerCount { get { return innerData.PlayerCount; } }

        public CPos[] SpawnPoints { get { return innerData.SpawnPoints; } }

        public MapGridT GridT { get { return innerData.GridT; } }

        public Rectangle Bounds { get { return innerData.Bounds; } }

        //public Bitmap Preview { get { return innerData.Preview; } }

        public MapStatus Status { get { return innerData.Status; } }

        public MapClassification Class { get { return innerData.Class; } }

        public MapVisibility Visibility { get { return innerData.Visibility; } }

        public Ruleset Rules { get { return innerData.Rules; } }

        public Bitmap Preview { get { return innerData.Preview; } }

        public bool InvalidCustomRules { get { return innerData.InvalidCustomRules; } }


        public bool DefinesUnsafeCustomRules
        {
            get
            {
                var force = innerData.Rules;
                return innerData.DefinesUnsafeCustomRules;
            }
        }
        public readonly string Uid;

        static readonly CPos[] NoSpawns = new CPos[] { };
        MapCache cache;
        ModData modData;

        public IReadOnlyPackage Package { get; private set; }
        IReadOnlyPackage parentPackage;
        public MapPreview(ModData modData,string uid,MapGridT gridT,MapCache cache)
        {
            this.cache = cache;
            this.modData = modData;

            Uid = uid;

            innerData = new InnerData
            {
                Title = "Unknown Map",
                Categories = new[] { "Unknown" },
                Author = "Unknown Author",
                TileSet = "Unknown",
                Players = null,
                PlayerCount = 0,
                SpawnPoints = NoSpawns,
                GridT = gridT,
                Bounds = Rectangle.Empty,
                Preview = null,
                Status = MapStatus.Unavailable,
                Visibility = MapVisibility.Lobby,
            
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="parent"></param>
        /// <param name="classification"></param>
        /// <param name="mapCompatibility"></param>
        /// <param name="gridT"></param>
        public void UpdateFromMap(IReadOnlyPackage p,IReadOnlyPackage parent,MapClassification classification,string[] mapCompatibility,MapGridT gridT)
        {
            Dictionary<string, MiniYaml> yaml;
            using(var yamlStream = p.GetStream("map.yaml"))
            {
                if (yamlStream == null)
                    throw new FileNotFoundException("Required file map.yaml not present in this map");

                yaml = new MiniYaml(null, MiniYaml.FromStream(yamlStream, "map.yaml")).ToDictionary();
            }

            Package = p;
            parentPackage = parent;


            var newData = innerData.Clone();
            newData.GridT = gridT;
            newData.Class = classification;

            MiniYaml temp;
            if(yaml.TryGetValue("MapFormat",out temp))
            {
                var format = FieldLoader.GetValue<int>("MapFormat", temp.Value);
                if(format != Map.SupportedMapFormat)
                {
                    throw new InvalidOperationException("Map format {0} is not supported.".F(format));
                }
            }
            if(yaml.TryGetValue("Title",out temp))
            {
                newData.Title = temp.Value;
            }
            if(yaml.TryGetValue("Categories",out temp))
            {
                newData.Categories = FieldLoader.GetValue<string[]>("Categories", temp.Value);
            }
            if (yaml.TryGetValue("Tileset", out temp))
                newData.TileSet = temp.Value;

            if (yaml.TryGetValue("Author", out temp))
                newData.Author = temp.Value;
            if (yaml.TryGetValue("Bounds", out temp))
                newData.Bounds = FieldLoader.GetValue<Rectangle>("Bounds", temp.Value);

            if (yaml.TryGetValue("Visibility", out temp))
                newData.Visibility = FieldLoader.GetValue<MapVisibility>("Visibility", temp.Value);

            string requiresMod = string.Empty;
            if (yaml.TryGetValue("RequiresMod", out temp))
                requiresMod = temp.Value;

            newData.Status = mapCompatibility == null || mapCompatibility.Contains(requiresMod) ? MapStatus.Available : MapStatus.Unavailable;

            try
            {
                MiniYaml actorDefinitions;
                if(yaml.TryGetValue("Actors",out actorDefinitions))
                {
                    var spawns = new List<CPos>();
                    foreach(var kv in actorDefinitions.Nodes.Where(d=>d.Value.Value == "mpspawn"))
                    {
                        var s = new ActorReference(kv.Value.Value, kv.Value.ToDictionary());
                        spawns.Add(s.InitDict.Get<LocationInit>().Value(null));
                    }
                    newData.SpawnPoints = spawns.ToArray();
                }
                else
                {
                    newData.SpawnPoints = new CPos[0];
                }
            }
            catch (Exception)
            {
                newData.SpawnPoints = new CPos[0];
                newData.Status = MapStatus.Unavailable;
            }


            try
            {
                //Player definitions may change if the map format changes
                MiniYaml playerDefinitions;
                if(yaml.TryGetValue("Players",out playerDefinitions))
                {
                    newData.Players = new MapPlayers(playerDefinitions.Nodes);
                    newData.PlayerCount = newData.Players.Players.Count(x => x.Value.Playable);
                }
            }
            catch (Exception)
            {
                newData.Status = MapStatus.Unavailable;
            }

            newData.SetRulesetGenerator(modData, () => {

                var ruleDefinitions = LoadRuleSection(yaml, "Rules");
                var weaponDefinitions = LoadRuleSection(yaml, "Weapons");
                var voiceDefinitions = LoadRuleSection(yaml, "Voices");
                var musicDefinitions = LoadRuleSection(yaml, "Music");
                var notificationDefinitions = LoadRuleSection(yaml, "Notifications");
                var sequenceDefinitions = LoadRuleSection(yaml, "Sequences");
                var modelSequenceDefinitions = LoadRuleSection(yaml, "ModelSequences");

                var rules = Ruleset.Load(modData, this, TileSet, ruleDefinitions, weaponDefinitions, voiceDefinitions, notificationDefinitions, musicDefinitions, sequenceDefinitions,modelSequenceDefinitions);

                var flagged = Ruleset.DefinesUnsafeCustomRules(modData,this,ruleDefinitions,weaponDefinitions,voiceDefinitions,notificationDefinitions,sequenceDefinitions);
                return Pair.New(rules, flagged);

            });

            if (Package.Contains("map.png"))
                using (var dataStream = p.GetStream("map.png"))
                    newData.Preview = BitmapFactory.DecodeStream(dataStream);

            innerData = newData;
        }

        public void PreloadRules()
        {
            var unused = Rules;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yaml"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        MiniYaml LoadRuleSection(Dictionary<string,MiniYaml> yaml,string section)
        {
            MiniYaml node;
            if (!yaml.TryGetValue(section, out node))
                return null;
            return node;
        }

        public void Delete()
        {

        }

        Sprite minimap;
        bool generatingMinimap;
        public Sprite GetMinimap()
        {
            if (minimap != null)
                return minimap;

            if (!generatingMinimap && Status == MapStatus.Available)
            {
                generatingMinimap = true;
                cache.CacheMinimap(this);
            }

            return null;
        }

        internal void SetMinimap(Sprite minimap)
        {
            this.minimap = minimap;
            generatingMinimap = false;
        }


        Stream IReadOnlyFileSystem.Open(string filename)
        {
            if (filename.Contains("|") && Package.Contains(filename))
                return Package.GetStream(filename);

            return modData.DefaultFileSystem.Open(filename);
        }

        bool IReadOnlyFileSystem.TryOpen(string filename, out Stream s)
        {
            if (!filename.Contains("|"))
            {
                s = Package.GetStream(filename);
                if (s != null)
                    return true;
            }
            return modData.DefaultFileSystem.TryOpen(filename, out s);
        }

        bool IReadOnlyFileSystem.TryGetPackageContaining(string path, out IReadOnlyPackage package, out string filename)
        {
            return modData.DefaultFileSystem.TryGetPackageContaining(path, out package, out filename);
        }

        bool IReadOnlyFileSystem.Exists(string filename)
        {
            if (!filename.Contains("|") && Package.Contains(filename))
                return true;
            return modData.DefaultFileSystem.Exists(filename);
        }

        public void Dispose()
        {
            if (Package != null)
            {
                Package.Dispose();
                Package = null;
            }
        }
    }
}