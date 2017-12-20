using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using EW.Primitives;
using EW.FileSystem;
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
        


        public MapCache(ModData modData)
        {
            this.modData = modData;

            var gridT = Exts.Lazy(() => modData.Manifest.Get<MapGrid>().Type);

            previews = new Cache<string, MapPreview>(uid => new MapPreview(modData, uid, gridT.Value, this));
            
            MapLocations = new ReadOnlyDictionary<IReadOnlyPackage, MapClassification>(mapLocations);


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
                var optional = name.StartsWith("~");
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
                            mapPackage = modData.ModFiles.OpenPackage(map, kv.Key);
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