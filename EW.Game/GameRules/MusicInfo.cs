using System;
using System.IO;
using EW.FileSystem;
namespace EW.GameRules
{
    public class MusicInfo
    {
        public readonly string Filename;
        public readonly string Title;
        public readonly bool Hidden;


        public int Length { get; private set; } //seconds

        public bool Exists { get; private set; }
        public MusicInfo(string key,MiniYaml value)
        {

        }

        public void Load(IReadOnlyFileSystem fileSystem)
        {
            Stream stream;
            if(!fileSystem.TryOpen(Filename,out stream))
            {
                return;
            }
        }
    }
}