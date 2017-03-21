using System;
using System.Collections.Generic;
using System.IO;

namespace RA.FileSystem
{

    /// <summary>
    /// 
    /// </summary>
    public interface IReadOnlyPackage : IDisposable
    {
        string Name { get; }

        IEnumerable<string> Contesnts { get; }

        Stream GetStream(string filename);

        bool Contains(string filename);
    }

    public interface IReadWritePackage : IReadOnlyPackage
    {
        void Update(string filename, byte[] contents);
        void Delete(string filename);
    }


    /// <summary>
    /// Ö»¶ÁÎÄ¼þ
    /// </summary>
    public interface IReadOnlyFileSystem
    {
        Stream Open(string filename);

        bool Exists(string filename);
    }



    /// <summary>
    /// 
    /// </summary>
    public class FileSystem:IReadOnlyFileSystem
    {


        readonly List<IReadOnlyPackage> modPackages = new List<IReadOnlyPackage>();






        public Stream Open(string filename)
        {
            Stream s = null;


            return s;
        }


        public bool Exists(string filename)
        {
            return false;
        }
    }


    
}