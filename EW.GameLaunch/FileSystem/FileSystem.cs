using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EW.Primitives;
namespace EW.FileSystem
{
    
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

        /// <summary>
        /// 显示挂载
        /// </summary>
        readonly Dictionary<string, IReadOnlyPackage> explicitMounts = new Dictionary<string, IReadOnlyPackage>();
        public IEnumerable<IReadOnlyPackage> MountedPackages { get { return mountedPackages.Keys; } }


        /// <summary>
        /// 
        /// </summary>
        readonly List<IReadOnlyPackage> modPackages = new List<IReadOnlyPackage>();

        /// <summary>
        /// 文件包索引
        /// </summary>
        Cache<string, List<IReadOnlyPackage>> fileIndex = new Cache<string, List<IReadOnlyPackage>>(_ => new List<IReadOnlyPackage>());
        /// <summary>
        /// 已安装Mod
        /// </summary>
        readonly IReadOnlyDictionary<string, Manifest> installedMods;

        IPackageLoader[] packageLoaders;

        public FileSystem(IReadOnlyDictionary<string,Manifest> installedMods,IPackageLoader[] packageLoaders=null)
        {
            this.installedMods = installedMods;
            this.packageLoaders = packageLoaders.Append(new ZipFileLoader()).ToArray();
        }


        public bool TryParsePackage(Stream stream,string filename,out IReadOnlyPackage package)
        {
            package = null;
            foreach (var packageLoader in packageLoaders)
                if (packageLoader.TryParsePackage(stream, filename, this, out package))
                    return true;

            return false;
        }

        /// <summary>
        /// 打开包文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public IReadOnlyPackage OpenPackage(string filename)
        { 
            //Raw directories are the easiest and one of the most common cases,so try these first
            var resolvedPath = Platform.ResolvePath(filename);
            if (!filename.Contains("|") && string.IsNullOrEmpty(Path.GetExtension(resolvedPath))) //&& Directory.Exists(resolvedPath))
                return new Folder(resolvedPath);
            
            
            //Children of another package require special handling
            IReadOnlyPackage parent;
            string subPath = null;
            if (TryGetPackageContaining(filename, out parent, out subPath))
                return parent.OpenPackage(subPath, this);

            //Try and open it normally
            IReadOnlyPackage package;
            var stream = Open(filename);
            if (TryParsePackage(stream, filename, out package))
                return package;

            //No package loaders took ownership of the stream,so clean it up
            stream.Dispose();
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


        /// <summary>
        /// 卸载所有已装载文件
        /// </summary>
        public void UnmountAll()
        {
            foreach (var package in mountedPackages.Keys)
            {
                if (!modPackages.Contains(package))
                    package.Dispose();
            }
            mountedPackages.Clear();
            explicitMounts.Clear();
            modPackages.Clear();

            fileIndex = new Cache<string, List<IReadOnlyPackage>>(_ => new List<IReadOnlyPackage>());
        }

        /// <summary>
        /// 装载文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="explicitName"></param>
        public void Mount(string name,string explicitName = null)
        {
            var optional = name.StartsWith("~");
            if (optional)
                name = name.Substring(1);
            try
            {
                IReadOnlyPackage package;
                if (name.StartsWith("$"))
                {
                    name = name.Substring(1);

                    Manifest mod;
                    if (!installedMods.TryGetValue(name, out mod))
                        throw new InvalidOperationException("Could not load mod '{0}'. Available mods:{1}".F(name, installedMods.Keys.JoinWith(", ")));

                    package = mod.Package;
                    modPackages.Add(package);

                }
                else
                {
                    package = OpenPackage(name);
                    if (package == null)
                        throw new InvalidOperationException("Could not open package '{0}',file not found or its format is not supported");
                }

                Mount(package, explicitName);
            }
            catch
            {
                if (!optional)
                    throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        /// <param name="explicitName"></param>
        public void Mount(IReadOnlyPackage package,string explicitName = null)
        {
            var mountCount = 0;
            if(mountedPackages.TryGetValue(package,out mountCount))
            {
                mountedPackages[package] = mountCount + 1;
                foreach(var filename in package.Contents)
                {
                    fileIndex[filename].Remove(package);
                    fileIndex[filename].Add(package);
                }
            }
            else
            {
                mountedPackages.Add(package, 1);
                if (explicitName != null)
                    explicitMounts.Add(explicitName, package);

                foreach (var filename in package.Contents)
                    fileIndex[filename].Add(package);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifest"></param>
        public void LoadFromManifest(Manifest manifest)
        {
            UnmountAll();
            foreach (var kv in manifest.Packages)
                Mount(kv.Key, kv.Value);
        }


        /// <summary>
        /// 打开文件流
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Stream Open(string filename)
        {
            Stream s = null;
            if (!TryOpen(filename, out s))
            {
                throw new FileNotFoundException("File not found:{0}".F(filename), filename);
            }

            return s;
        }


        public bool Exists(string filename)
        {
            var explicitSplit = filename.IndexOf('|');
            if (explicitSplit > 0)
            {
                IReadOnlyPackage explicitPackage;
                if (explicitMounts.TryGetValue(filename.Substring(0, explicitSplit), out explicitPackage))
                    if (explicitPackage.Contains(filename.Substring(explicitSplit + 1)))
                        return true;
            }

            return fileIndex.ContainsKey(filename);
        }

        public static string ResolveAssemblyPath(string path,Manifest manifest,InstalledMods installedMods)
        {
            var explicitSplit = path.IndexOf('|');
            if (explicitSplit > 0)
            {
                var parent = path.Substring(0, explicitSplit);
                var filename = path.Substring(explicitSplit + 1);

                var parentPath = manifest.Packages.FirstOrDefault(kv => kv.Value == parent).Key;
                if (parentPath == null)
                    return null;

                if (parentPath.StartsWith("$", StringComparison.Ordinal))
                {

                }
                else
                {
                    path = Path.Combine(parentPath, filename);
                }
            }
            var resolvedPath = Platform.ResolvePath(path);
            return resolvedPath;
        }
    }


    
}