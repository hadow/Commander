using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using EW.FileSystem;
using EW.Xna.Platforms;
using EW.Primitives;
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

        Vector2 Offset { get; }
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
        public readonly SheetBuilder SheetBuilder;

        readonly ISpriteLoader[] loaders;

        readonly IReadOnlyFileSystem fileSystem;

        readonly Dictionary<string, List<Sprite[]>> sprites = new Dictionary<string, List<Sprite[]>>();

        public SpriteCache(IReadOnlyFileSystem fileSystem,ISpriteLoader[] loaders,SheetBuilder sheetBuilder)
        {
            this.SheetBuilder = sheetBuilder;
            this.loaders = loaders;
            this.fileSystem = fileSystem;

        }
        /// <summary>
        /// Returns the first set of sprites with the given filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Sprite[] this[string filename]
        {
            get
            {
                var allSprites = sprites.GetOrAdd(filename);
                var sprite = allSprites.FirstOrDefault();
                return sprite ?? LoadSprite(filename, allSprites);
            }
        }

        /// <summary>
        /// Loads and caches a new instance of sprites with the given filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Sprite[] Reload(string filename)
        {
            return LoadSprite(filename, sprites.GetOrAdd(filename));
        }

        Sprite[] LoadSprite(string filename,List<Sprite[]> cache)
        {
            var sprite = SpriteLoader.GetSprites(fileSystem, filename, loaders, SheetBuilder);
            cache.Add(sprite);
            return sprite;
        }

        /// <summary>
        /// Returns all instance of sets of sprites with the given filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public IEnumerable<Sprite[]> AllCached(string filename)
        {
            return sprites.GetOrAdd(filename);
        }
    }

    public class FrameCache
    {
        readonly Cache<string, ISpriteFrame[]> frames;

        public FrameCache(IReadOnlyFileSystem fileSystem,ISpriteLoader[] loaders)
        {
            frames = new Cache<string, ISpriteFrame[]>(filename => SpriteLoader.GetFrames(fileSystem, filename, loaders));
        }

        public ISpriteFrame[] this[string filename] { get { return frames[filename]; } }
    }

    public static class SpriteLoader
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="filename"></param>
        /// <param name="loaders"></param>
        /// <param name="sheetBuilder"></param>
        /// <returns></returns>
        public static Sprite[] GetSprites(IReadOnlyFileSystem fileSystem,string filename,ISpriteLoader[] loaders,SheetBuilder sheetBuilder)
        {
            return GetFrames(fileSystem, filename, loaders).Select(a=>sheetBuilder.Add(a)).ToArray();
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