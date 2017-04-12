using System;
using System.IO;
namespace EW.Xna.Platforms.Utilities
{
    internal static class FileHelpers
    {

        public static readonly char ForwardSlash = '/';
        public static readonly string ForwardSlashString = new string(ForwardSlash, 1);
        public static readonly char BackwardSlash = '\\';

        public static readonly char NotSeparator = Path.DirectorySeparatorChar == BackwardSlash ? ForwardSlash : BackwardSlash;
        public static readonly char Separator = Path.DirectorySeparatorChar;
    }
}