using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using EW.Primitives;
using EW.FileSystem;
using EW.Support;
using EW.Graphics;
namespace EW
{
    /// <summary>
    /// ª∫¥ÊµÿÕº
    /// </summary>
    public sealed class MapCache:IEnumerable<MapPreview>,IDisposable
    {
        public static readonly MapPreview UnknownMap = new MapPreview(null, null, MapGridT.Rectangular, null);

        public readonly IReadOnlyDictionary<IReadOnlyPackage, MapClassification> MapLocations;

        readonly Dictionary<IReadOnlyPackage,MapClassification> mapLocations = new Dictionary<IReadOnlyPackage, MapClassification>();
        readonly Cache<string, MapPreview> previews;

        readonly ModData modData;

        Thread previewLoaderThread;

        bool previewLoaderThreadShutDown = true;

        Queue<MapPreview> generateMinimap = new Queue<MapPreview>();

        object syncRoot = new object();

        readonly SheetBuilder sheetBuilder;
        public MapCache(ModData modData)
        {
            this.modData = modData;

            var gridT = Exts.Lazy(() => modData.Manifest.Get<MapGrid>().Type);

            previews = new Cache<string, MapPreview>(uid => new MapPreview(modData, uid, gridT.Value, this));

            sheetBuilder = new SheetBuilder(SheetT.BGRA);
            MapLocations = new ReadOnlyDictionary<IReadOnlyPackage, MapClassification>(mapLocations);


        }


        void LoadAsyncInternal()
        {
            //Log.Write("debug", "MapCache.LoadAsyncInternal started");

            // Milliseconds to wait on one loop when nothing to do
            var emptyDelay = 50;

            // Keep the thread alive for at least 5 seconds after the last minimap generation
            var maxKeepAlive = 5000 / emptyDelay;
            var keepAlive = maxKeepAlive;

            for (; ; )
            {
                List<MapPreview> todo;
                lock (syncRoot)
                {
                    todo = generateMinimap.Where(p => p.GetMinimap() == null).ToList();
                    generateMinimap.Clear();
                    if (keepAlive > 0)
                        keepAlive--;
                    if (keepAlive == 0 && todo.Count == 0)
                    {
                        previewLoaderThreadShutDown = true;
                        break;
                    }
                }

                if (todo.Count == 0)
                {
                    Thread.Sleep(emptyDelay);
                    continue;
                }
                else
                    keepAlive = maxKeepAlive;

                // Render the minimap into the shared sheet
                foreach (var p in todo)
                {
                    if (p.Preview != null)
                    {
                        WarGame.RunAfterTick(() =>
                        {
                            try
                            {
                                p.SetMinimap(sheetBuilder.Add(p.Preview));
                            }
                            catch (Exception e)
                            {
                                //Log.Write("debug", "Failed to load minimap with exception: {0}", e);
                            }
                        });
                    }

                    // Yuck... But this helps the UI Jank when opening the map selector significantly.
                    Thread.Sleep(Environment.ProcessorCount == 1 ? 25 : 5);
                }
            }

            // The buffer is not fully reclaimed until changes are written out to the texture.
            // We will access the texture in order to force changes to be written out, allowing the buffer to be freed.
            WarGame.RunAfterTick(() =>
            {
                sheetBuilder.Current.ReleaseBuffer();
                sheetBuilder.Current.GetTexture();
            });
            //Log.Write("debug", "MapCache.LoadAsyncInternal ended");
        }

        public void CacheMinimap(MapPreview preview)
        {
            bool launchPreviewLoaderThread;
            lock (syncRoot)
            {
                generateMinimap.Enqueue(preview);
                launchPreviewLoaderThread = previewLoaderThreadShutDown;
                previewLoaderThreadShutDown = false;
            }

            if (launchPreviewLoaderThread)
                WarGame.RunAfterTick(() =>
                {
                    // Wait for any existing thread to exit before starting a new one.
                    if (previewLoaderThread != null)
                        previewLoaderThread.Join();

                    previewLoaderThread = new Thread(LoadAsyncInternal)
                    {
                        Name = "Map Preview Loader",
                        IsBackground = true
                    };
                    previewLoaderThread.Start();
                });
        }

        /// <summary>
        /// º”‘ÿµÿÕº
        /// </summary>
        public void LoadMaps()
        {
            if (!this.modData.Manifest.Contains<MapGrid>())
                return;

            //Enumerate map directories
            foreach (var kv in modData.Manifest.MapFolders)
            {
                var name = kv.Key;
                var classification = string.IsNullOrEmpty(kv.Value) ? MapClassification.Unknown :
                    Enum<MapClassification>.Parse(kv.Value);

                IReadOnlyPackage package;
                var optional = name.StartsWith("~",StringComparison.Ordinal);
                if (optional)
                    name = name.Substring(1);

                try
                {
                    package = modData.ModFiles.OpenPackage(name);
                }
                catch
                {
                    if (optional)
                        continue;
                    throw;
                }
                mapLocations.Add(package, classification);
            }
            var mapGrid = this.modData.Manifest.Get<MapGrid>();
            foreach(var kv in MapLocations)
            {
                foreach(var map in kv.Key.Contents)
                {
                    IReadOnlyPackage mapPackage = null;
                    try
                    {
                        using(new Support.PerfTimer(map))
                        {
                            mapPackage = kv.Key.OpenPackage(map, modData.ModFiles);
                            if (mapPackage == null)
                                continue;

                            var uid = Map.ComputeUID(mapPackage);
                            previews[uid].UpdateFromMap(mapPackage, kv.Key, kv.Value, modData.Manifest.MapCompatibility, mapGrid.Type);
                        }
                    }
                    catch(Exception e)
                    {
                        if (mapPackage != null)
                            mapPackage.Dispose();

                    }
                }
            }
        }

        public string ChooseInitialMap(string initialUid,MersenneTwister random)
        {
            if(string.IsNullOrEmpty(initialUid) || previews[initialUid].Status != MapStatus.Available)
            {
                var selected = previews.Values.Where(IsSuitableInitialMap).RandomOrDefault(random) ??
                    previews.Values.First(m => m.Status == MapStatus.Available && m.Visibility.HasFlag(MapVisibility.Lobby));
                return selected.Uid;
            }

            return initialUid;
        }

        bool IsSuitableInitialMap(MapPreview map)
        {
            if (map.Status != MapStatus.Available || !map.Visibility.HasFlag(MapVisibility.Lobby))
                return false;

            //Other map types may have confusing settings or gameplay
            if (!map.Categories.Contains("Conquest"))
                return false;

            if (map.Players.Players.Any(x => !x.Value.AllowBots))
                return false;

            //Large maps expose unfortunate performance problems
            if (map.Bounds.Width > 128 || map.Bounds.Height > 128)
                return false;

            return true;

        }
        public MapPreview this[string key]
        {
            get { return previews[key]; }
        }


        public IEnumerator<MapPreview> GetEnumerator()
        {
            return previews.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            foreach (var p in previews.Values)
                p.Dispose();
        }
    }
}