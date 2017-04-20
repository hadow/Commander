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
    public class InstalledMods:EW.Primitives.IReadOnlyDictionary<string,Manifest>
    {

        readonly Dictionary<string, Manifest> mods;


        public InstalledMods(string customPath)
        {
            mods = GetInstalledMods(customPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static IEnumerable<Pair<string,string>> GetCandidateMods()
        {
            var basePath = Platform.ResolvePath(Path.Combine(".", "mods"));
            
            //var directories = Directory.GetDirectories(basePath);
            //var mods = directories.Select(x => Pair.New(x.Substring(basePath.Length + 1), x)).ToList();

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customModPath"></param>
        /// <returns></returns>
        static Dictionary<string,Manifest> GetInstalledMods(string customModPath)
        {

            var ret = new Dictionary<string, Manifest>();


            Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
            foreach(var ef in externalFilesDirs)
            {
                System.Diagnostics.Debug.Print(ef.AbsolutePath);
            }
            //var candidates = GetCandidateMods();

            //if (customModPath != null)
            //    candidates = candidates.Append(Pair.New(Path.GetFileNameWithoutExtension(customModPath), customModPath));

            //foreach(var pair in candidates)
            //{
            //    var mod = LoadMod(pair.First, pair.Second);
            //    if (mod != null)
            //        ret[pair.First] = mod;
            //}

            return ret;
        }


        static Manifest LoadMod(string id,string path)
        {
            IReadOnlyPackage package = null;
            try
            {
                if (Directory.Exists(path))
                    package = new Folder(path);
                else
                {
                    try
                    {
                        using (var fileStream = File.OpenRead(path))
                            package = new ZipFile(fileStream, path);
                    }
                    catch
                    {
                        throw new InvalidOperationException(path + " is not a valid mod package");
                    }
                }

                if (!package.Contains("mod.yaml"))
                    throw new InvalidDataException(path + " is not a valid mod package");

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