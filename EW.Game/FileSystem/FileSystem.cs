using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EW.Primitives;
namespace EW.FileSystem
{

    /// <summary>
    /// 
    /// </summary>
    public interface IReadOnlyPackage : IDisposable
    {
        string Name { get; }

        IEnumerable<string> Contents { get; }

        Stream GetStream(string filename);

        bool Contains(string filename);
    }

    public interface IReadWritePackage : IReadOnlyPackage
    {
        void Update(string filename, byte[] contents);
        void Delete(string filename);
    }


    /// <summary>
    /// 只读文件
    /// </summary>
    public interface IReadOnlyFileSystem
    {
        Stream Open(string filename);

        bool TryGetPackageContaining(string path, out IReadOnlyPackage package, out string filename);

        bool TryOpen(string filename, out Stream s);
        bool Exists(string filename);
    }
    

    /// <summary>
    /// 
    /// </summary>
    public class FileSystem:IReadOnlyFileSystem
    {

        /// <summary>
        /// 
        /// </summary>
        readonly Dictionary<IReadOnlyPackage, int> mountedPackages = new Dictionary<IReadOnlyPackage, int>();
        readonly Dictionary<string, IReadOnlyPackage> explicitMounts = new Dictionary<string, IReadOnlyPackage>();
        public IEnumerable<IReadOnlyPackage> MountedPackages { get { return mountedPackages.Keys; } }


        /// <summary>
        /// 
        /// </summary>
        readonly List<IReadOnlyPackage> modPackages = new List<IReadOnlyPackage>();

        Cache<string, List<IReadOnlyPackage>> fileIndex = new Cache<string, List<IReadOnlyPackage>>(_ => new List<IReadOnlyPackage>());
        /// <summary>
        /// 已安装Mod
        /// </summary>
        readonly EW.Primitives.IReadOnlyDictionary<string, Manifest> installedMods;

        public FileSystem(EW.Primitives.IReadOnlyDictionary<string,Manifest> installedMods)
        {
            this.installedMods = installedMods;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public IReadOnlyPackage OpenPackage(string filename)
        {
            if (filename.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                return new ZipFile(this, filename);
            IReadOnlyPackage parent;
            string subPath = null;
            if (TryGetPackageContaining(filename, out parent, out subPath))
                return OpenPackage(subPath, parent);
            return new Folder(filename);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public IReadOnlyPackage OpenPackage(string filename,IReadOnlyPackage parent)
        {
            if(filename.EndsWith(".zip",StringComparison.InvariantCultureIgnoreCase) || filename.EndsWith(".oramap", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var s = parent.GetStream(filename))
                    return new ZipFile(s, filename, parent);
            }

            if (parent is ZipFile)
                return new ZipFolder(this, (ZipFile)parent, filename, filename);
            if(parent is ZipFolder)
            {
                var folder = (ZipFolder)parent;

            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="package"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool TryGetPackageContaining(string path,out IReadOnlyPackage package,out string filename)
        {
            var explicitSplit = path.IndexOf('|');
            if(explicitSplit>0 && explicitMounts.TryGetValue(path.Substring(0,explicitSplit),out package))
            {
                filename = path.Substring(explicitSplit + 1);
                return true;
            }

            package = fileIndex[path].LastOrDefault(x => x.Contains(path));
            filename = path;

            return package != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool TryOpen(string filename,out Stream s)
        {
            var explicitSplit = filename.IndexOf('|');
            if (explicitSplit > 0)
            {
                IReadOnlyPackage explicitPackage;
                if(explicitMounts.TryGetValue(filename.Substring(0,explicitSplit),out explicitPackage))
                {
                    s = explicitPackage.GetStream(filename.Substring(explicitSplit + 1));
                    if (s != null)
                    {
                        return true;
                    }
                }
            }

            s = GetStreamCache(filename);
            if (s != null)
                return true;

            var package = mountedPackages.Keys.LastOrDefault(x => x.Contains(filename));
            if (package != null)
            {
                s = package.GetStream(filename);
                return s != null;
            }

            s = null;
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        Stream GetStreamCache(string filename)
        {
            var package = fileIndex[filename].LastOrDefault(x => x.Contains(filename));

            if (package != null)
                return package.GetStream(filename);
            return null;
        }

        public Stream Open(string filename)
        {
            Stream s = null;


            return s;
        }


        public bool Exists(string filename)
        {
            return fileIndex.ContainsKey(filename);
        }
    }


    
}