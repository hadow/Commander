using System;
using System.IO;
namespace EW
{

    public enum PlatformT { Unknown,Android,Ios,Windows,Linux,OSX}
    public static class Platform
    {
        
        public static string GameDir { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        static Lazy<string> supportDir = Exts.Lazy(GetSupportDir);

        public static string SupportDir { get { return supportDir.Value; } }

        static string GetSupportDir()
        {
            if (Directory.Exists("Support"))
                return "Support" + Path.DirectorySeparatorChar;

            var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir + Path.DirectorySeparatorChar;
        }

        static PlatformT GetCurrentPlatform()
        {
            
            return PlatformT.Unknown;
        }

        public static string ResolvePath(string path)
        {
            path = path.TrimEnd(new char[] { ' ', '\t' });

            if (path.StartsWith("^", StringComparison.Ordinal))
            {
                path = SupportDir + path.Substring(1);
            }

            if (path == ".")
                return GameDir;

            if (path.StartsWith("./", StringComparison.Ordinal) || path.StartsWith(".\\", StringComparison.Ordinal))
                path = GameDir + path.Substring(2);

            return path;
        }

    }
}