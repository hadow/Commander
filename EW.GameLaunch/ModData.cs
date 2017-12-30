using System;
using System.Collections.Generic;
using System.IO;
using EW.Graphics;
using EW.FileSystem;
using System.Linq;
namespace EW
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class ModData:IDisposable
    {
        public IEnumerable<string> Languages { get; private set; }

        int initialThreadId;

        internal bool IsOnMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == initialThreadId; }
        }
        public readonly Manifest Manifest;

        public readonly ObjectCreator ObjectCreator;

        public readonly MapCache MapCache;

        public readonly IPackageLoader[] PackageLoaders;
        public readonly ISpriteLoader[] SpriteLoaders;
        public readonly ISpriteSequenceLoader SpriteSequenceLoader;
        public readonly IModelSequenceLoader ModelSequenceLoader;
        public ILoadScreen LoadScreen { get; private set; }
        /// <summary>
        /// 默认规则 
        /// </summary>
        readonly Lazy<Ruleset> defaultRules;
        
        public Ruleset DefaultRules { get { return defaultRules.Value; } }
        public EW.FileSystem.FileSystem ModFiles;
        public IReadOnlyFileSystem DefaultFileSystem { get { return ModFiles; } }

        readonly Lazy<IReadOnlyDictionary<string, TileSet>> defaultTileSets;

        public IReadOnlyDictionary<string,TileSet> DefaultTileSets
        {
            get
            {
                return defaultTileSets.Value;
            }
        }

        readonly Lazy<IReadOnlyDictionary<string, SequenceProvider>> defaultSequences;

        public IReadOnlyDictionary<string,SequenceProvider> DefaultSequences
        {
            get { return defaultSequences.Value; }
        }

        public ModData(Manifest mod,InstalledMods mods,bool useLoadScreen = false)
        {
            Languages = new string[0];
            
            //local copy of the manifest
            Manifest = new Manifest(mod.Id, mod.Package);
            ObjectCreator = new ObjectCreator(Manifest, mods);
            PackageLoaders = ObjectCreator.GetLoaders<IPackageLoader>(Manifest.PackageFormats, "package");
            ModFiles = new FileSystem.FileSystem(mods,PackageLoaders);
            ModFiles.LoadFromManifest(Manifest);
            Manifest.LoadCustomData(ObjectCreator);
            if (useLoadScreen)
            {
                LoadScreen = ObjectCreator.CreateObject<ILoadScreen>(Manifest.LoadScreen.Value);
                LoadScreen.Init(this, Manifest.LoadScreen.ToDictionary(my => my.Value));
                LoadScreen.Display();
            }


            MapCache = new MapCache(this);

            SpriteLoaders = GetLoaders<ISpriteLoader>(Manifest.SpriteFormats, "sprite");

            var sequenceFormat = Manifest.Get<SpriteSequenceFormat>();
            var sequenceLoader = ObjectCreator.FindType(sequenceFormat.Type + "Loader");
            var sequenceCtor = sequenceLoader != null ? sequenceLoader.GetConstructor(new[] { typeof(ModData) }) : null;
            if (sequenceLoader == null || !sequenceLoader.GetInterfaces().Contains(typeof(ISpriteSequenceLoader)) || sequenceCtor == null)
                throw new InvalidOperationException("Unable to find a sequence loader for type '{0}'.".F(sequenceFormat.Type));
            SpriteSequenceLoader = (ISpriteSequenceLoader)sequenceCtor.Invoke(new[] { this });
            SpriteSequenceLoader.OnMissingSpriteError =s=> { Console.WriteLine(s); };

            var modelFormat = Manifest.Get<ModelSequenceFormat>();
            var modelLoader = ObjectCreator.FindType(modelFormat.Type + "Loader");
            var modelCtor = modelLoader != null ? modelLoader.GetConstructor(new[] { typeof(ModData) }) : null;

            if (modelCtor == null || !modelLoader.GetInterfaces().Contains(typeof(IModelSequenceLoader)) || modelCtor == null)
                throw new InvalidOperationException("Unable to find a model loader for type '{0}'".F(modelFormat.Type));




            ModelSequenceLoader = (IModelSequenceLoader)modelCtor.Invoke(new[] { this });
            ModelSequenceLoader.OnMissingModelError = s => { };

            defaultRules = Exts.Lazy(() => Ruleset.LoadDefaults(this));

            //地形贴片集
            defaultTileSets = Exts.Lazy(() =>
            {
                var items = new Dictionary<string, TileSet>();
                foreach(var file in Manifest.TileSets)
                {
                    var t = new TileSet(DefaultFileSystem, file);
                    items.Add(t.Id, t);
                }
                return (IReadOnlyDictionary<string,TileSet>)(new ReadOnlyDictionary<string, TileSet>(items));
            });

            //序列集
            defaultSequences = Exts.Lazy(() => {

                var items = DefaultTileSets.ToDictionary(t => t.Key, t => new SequenceProvider(DefaultFileSystem, this, t.Value, null));
                return (IReadOnlyDictionary<string,SequenceProvider>)(new ReadOnlyDictionary<string, SequenceProvider>(items));

            });

            initialThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSystem"></param>
        public void InitializeLoaders(IReadOnlyFileSystem fileSystem)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TLoader"></typeparam>
        /// <param name="formats"></param>
        /// <param name="name"></param>
        /// <returns></returns>

        TLoader[] GetLoaders<TLoader>(IEnumerable<string> formats,string name)
        {
            var loaders = new List<TLoader>();
            foreach(var format in formats)
            {
                var loader = ObjectCreator.FindType(format + "Loader");
                if (loader == null || !loader.GetInterfaces().Contains(typeof(TLoader)))
                    throw new InvalidOperationException("Unable to find a {0} loader for type '{1}'.".F(name, format));

                loaders.Add((TLoader)ObjectCreator.CreateBasic(loader));
            }
            return loaders.ToArray();
        }

        /// <summary>
        /// 地图准备
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Map PrepareMap(string uid)
        {
            if (MapCache[uid].Status != MapStatus.Available)
                throw new InvalidDataException("Invalid map uid:{0}".F(uid));

            Map map;
            using (new Support.PerfTimer("Map"))
                map = new Map(this, MapCache[uid].Package);

            using (new Support.PerfTimer("Map.Music"))
                foreach (var entry in map.Rules.Music)
                    entry.Value.Load(map);


            return map;
        }


        internal void HandleLoadingProgress()
        {
            if(LoadScreen != null && IsOnMainThread)
            {
                LoadScreen.Display();
            }
        }





        public void Dispose()
        {
            MapCache.Dispose();

            if (ObjectCreator != null)
                ObjectCreator.Dispose();
        }

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    MapCache.Dispose();
        //}


    }



    public interface ILoadScreen : IDisposable
    {
        void Init(ModData m, Dictionary<string, string> info);

        void Display();

        bool RequiredContentIsInstalled();

        void StartGame(Arguments args);
    }
}