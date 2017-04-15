using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.FileSystem;
using EW.Primitives;
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

        /// <summary>
        /// ƒ¨»œπÊ‘Ú 
        /// </summary>
        readonly Lazy<Ruleset> defaultRules;
        
        public Ruleset DefaultRules { get { return defaultRules.Value; } }
        public EW.FileSystem.FileSystem ModFiles;
        public IReadOnlyFileSystem DefaultFileSystem { get { return ModFiles; } }

        readonly Lazy<EW.Primitives.IReadOnlyDictionary<string, TileSet>> defaultTileSets;

        public EW.Primitives.IReadOnlyDictionary<string,TileSet> DefaultTileSets
        {
            get
            {
                return defaultTileSets.Value;
            }
        }

        readonly Lazy<EW.Primitives.IReadOnlyDictionary<string, SequenceProvider>> defaultSequences;

        public EW.Primitives.IReadOnlyDictionary<string,SequenceProvider> DefaultSequences
        {
            get { return defaultSequences.Value; }
        }


        public ModData(Manifest mod,InstalledMods mods,bool useLoadScreen = false)
        {
            Languages = new string[0];

            ModFiles = new FileSystem.FileSystem(mods);

            Manifest = new Manifest(mod.Id, mod.Package);
            ModFiles.LoadFromManifest(Manifest);

            ObjectCreator = new ObjectCreator(Manifest, ModFiles);
            Manifest.LoadCustomData(ObjectCreator);

            MapCache = new MapCache(this);

            defaultRules = Exts.Lazy(() => Ruleset.LoadDefaults(this));

            defaultTileSets = Exts.Lazy(() =>
            {
                var items = new Dictionary<string, TileSet>();
                foreach(var file in Manifest.TileSets)
                {
                    var t = new TileSet(DefaultFileSystem, file);
                    items.Add(t.Id, t);
                }
                return (EW.Primitives.IReadOnlyDictionary<string,TileSet>)(new ReadOnlyDictionary<string, TileSet>(items));
            });

            initialThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }











        public void Dispose()
        {

        }


    }
}