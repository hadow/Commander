using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using EW.FileSystem;
namespace EW.Graphics
{
    public interface ISpriteFrame
    {
        /// <summary>
        /// Size of the frame's 'Data'.
        /// </summary>
        Size Size { get; }

        Size FrameSize { get; }

        byte[] Data { get; }

        bool DisableExportPadding { get; }
    }

    public interface ISpriteLoader
    {
        bool TryParseSprite(Stream s, out ISpriteFrame[] frames);
    }


    /// <summary>
    /// Sprite »º´æ
    /// </summary>
    public class SpriteCache
    {
        readonly ISpriteLoader[] loaders;

        readonly IReadOnlyFileSystem fileSystem;

        readonly Dictionary<string, List<Sprite[]>> sprites = new Dictionary<string, List<Sprite[]>>();

        public SpriteCache(IReadOnlyFileSystem fileSystem,ISpriteLoader[] loaders)
        {
            this.loaders = loaders;
            this.fileSystem = fileSystem;

        }

        public Sprite[] this[string filename]
        {
            get
            {
                var allSprites = sprites.GetOrAdd(filename);
                var sprite = allSprites.FirstOrDefault();
                return sprite ?? Loadsp
            }
        }

        Sprite[] LoadSprite(string filename,List<Sprite[]> cache)
        {

        }
    }

    public static class SpriteLoader
    {

        public static Sprite[] GetSprites(IReadOnlyFileSystem fileSystem,string filename,ISpriteLoader[] loaders)
        {
            return GetFrames(fileSystem, filename, loaders);
        }

        public static ISpriteFrame[] GetFrames(IReadOnlyFileSystem fileSystem,string filename,ISpriteLoader[] loaders)
        {
            using(var stream = fileSystem.Open(filename))
            {
                var spriteFrames = GetFrames(stream, loaders);
                if (spriteFrames == null)
                    throw new InvalidOperationException(filename + " is not a valid sprite file!");

                return spriteFrames;
            }
        }

        public static ISpriteFrame[] GetFrames(Stream stream,ISpriteLoader[] loaders)
        {
            ISpriteFrame[] frames;
            foreach(var loader in loaders)
            {
                if (loader.TryParseSprite(stream, out frames))
                    return frames;
            }

            return null;
        }


    }
}