using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EW.FileSystem;
using EW.Primitives;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class InstalledMods:IReadOnlyDictionary<string,Manifest>
    {

        readonly Dictionary<string, Manifest> mods;


        public InstalledMods(string customPath)
        {
            mods = GetInstalledMods(customPath);
        }


        /// <summary>
        /// 可供选择的Mod
        /// </summary>
        /// <returns></returns>
        static IEnumerable<Pair<string,string>> GetCandidateMods()
        {
            var basePath = Platform.ResolvePath(Path.Combine(".", "mods"));
            //string[] directories = Directory.GetDirectories(basePath);
            string[] directories = Android.App.Application.Context.Assets.List(basePath);
            var mods = directories.Select(x => Pair.New(x, Path.Combine(basePath,x))).ToList();

            return mods;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customModPath"></param>
        /// <returns></returns> 
        static Dictionary<string,Manifest> GetInstalledMods(string customModPath)
        {

            var ret = new Dictionary<string, Manifest>();
            var candidates = GetCandidateMods();

            //if (customModPath != null)
            //    candidates = candidates.Append(Pair.New(Path.GetFileNameWithoutExtension(customModPath), customModPath));

            foreach(var pair in candidates)
            {
                var mod = LoadMod(pair.First, pair.Second);
                if (mod != null)
                    ret[pair.First] = mod;
            }
            return ret;
        }


        static Manifest LoadMod(string id,string path)
        {
            IReadOnlyPackage package = null;
            try
            {
                package = new Folder(path);
                if (package.Contains("mod.yaml"))
                {
                    using (var stream = package.GetStream("icon.png"))
                        if(stream !=null)
                        {
                            Console.Write("icon data:" + stream.Length);
                        }
                }
                return new Manifest(id, package);

            }
            catch (Exception)
            {
                if (package != null)
                    package.Dispose();
                return null;
            }
        }

        public Manifest this[string key] { get { return mods[key]; } }

        public int Count { get { return mods.Count; } }

        public ICollection<string> Keys { get { return mods.Keys; } }

        public ICollection<Manifest> Values { get { return mods.Values; } }

        public bool ContainsKey(string key) { return mods.ContainsKey(key); }

        public bool TryGetValue(string key,out Manifest value)
        {
            return mods.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string,Manifest>> GetEnumerator()
        {
            return mods.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return mods.GetEnumerator();
        }

    }
}