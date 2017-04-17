using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using EW.FileSystem;
namespace EW.Graphics
{
    public interface ISpriteFrame
    {
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
    /// 
    /// </summary>
    public class SpriteCache
    {
        readonly ISpriteLoader[] loaders;

        readonly IReadOnlyFileSystem fileSystem;

        readonly Dictionary<string, List<Sprite[]>> sprites = new Dictionary<string, List<Sprite[]>>();
    }

    public static class SpriteLoader
    {




    }
}