using System;
using System.Collections.Generic;

using RA.FileSystem;
namespace RA
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class ModData:IDisposable
    {

        public readonly ObjectCreator ObjectCreator;

        public RA.FileSystem.FileSystem ModFiles;
        public IReadOnlyFileSystem DefaultFileSystem { get { return ModFiles; } }




        public void Dispose()
        {

        }


    }
}