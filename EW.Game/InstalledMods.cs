using System;
using System.Collections.Generic;
using System.IO;
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

        static IEnumerable<Pair<string,string>> GetCandidateMods()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customModPath"></param>
        /// <returns></returns>
        static Dictionary<string,Manifest> GetInstalledMods(string customModPath)
        {



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
    }
}