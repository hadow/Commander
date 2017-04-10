using System;
using System.Collections.Generic;
using System.IO;
using EW.FileSystem;
using EW.Mobile.Platforms;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class Map:IReadOnlyFileSystem
    {
        readonly ModData modData;

        public readonly MapGrid Grid;

        public Vector2 MapSize { get; private set; }
        public Stream Open(string filename)
        {
            return modData.DefaultFileSystem.Open(filename);
        }

        public bool Exists(string filename)
        {
            return modData.DefaultFileSystem.Exists(filename);
        }

    }
}