using System;
using System.IO;
namespace EW
{

    public enum PlatformT { Unknown,Android,Ios,Windows,Linux,OSX}
    public static class Platform
    {
        private static readonly string RootDirectory = "Content";
        public static string GameDir { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        static Lazy<string> supportDir = Exts.Lazy(GetSupportDir);

        public static string SupportDir { get { return supportDir.Value; } }

        static string GetSupportDir()
        {
            //if (Directory.Exists("Support"))
            //    return "Support" + Path.DirectorySeparatorChar;
            //Java.IO.File externalFile = Android.App.Application.Context.GetExternalFilesDir(null);
            //var files = externalFile.ListFiles();
            //Android.Content.Res.Resources resources = Android.App.Application.Context.Resources;
            //string rootDirector = Directory.GetDirectoryRoot(externalFile.Path);
            //string[] directories = Directory.GetDirectories(externalFile.Path);
            //string[] fileList = Android.App.Application.Context.FileList();
            Java.IO.File fieDir =  Android.App.Application.Context.FilesDir;
            //if (File.Exists(fieDir.AbsolutePath))
            //{
            //    System.Diagnostics.Debug.Print(fieDir.Path);
            //}
            var directories = Directory.GetDirectories(fieDir.Path);
            //var dirContent = directories[3];
            //directories = Directory.GetDirectories(dirContent);
            //if (File.Exists(Path.Combine(dirContent, "settings.yaml")))
            //{
            //    System.Diagnostics.Debug.Print(Path.Combine(dirContent,"settings.yaml"));
            //}
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            dir = Path.Combine(dir, RootDirectory);

            if (!Directory.Exists(Path.Combine(dir,"mods/cnc")))
                Directory.CreateDirectory(Path.Combine(dir, "mods/cnc"));

            return dir;//+ Path.DirectorySeparatorChar;
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
                //path = Path.Combine(RootDirectory, path.Substring(1));
            }

            //if (Path.IsPathRooted(path))
            //    throw new ArgumentException("Invalid filename");


            //var uri = new Uri("file:///" + path);
            //path = uri.LocalPath;
            //path = path.Substring(1);
            //path = path.Replace(Path.DirectorySeparatorChar == '\\' ? '/' : '\\', Path.DirectorySeparatorChar);
            if (path == ".")
                return GameDir;

            if (path.StartsWith("./", StringComparison.Ordinal) || path.StartsWith(".\\", StringComparison.Ordinal))
                path = SupportDir + path.Substring(1);

            return path;
        }

    }
}