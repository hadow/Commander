using System;
using System.Collections.Generic;

using EW.FileSystem;
namespace EW
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class ModData:IDisposable
    {

        public readonly ObjectCreator ObjectCreator;

        public EW.FileSystem.FileSystem ModFiles;
        public IReadOnlyFileSystem DefaultFileSystem { get { return ModFiles; } }




        public void Dispose()
        {

        }


    }
}