using System;
using System.IO;
using System.Linq;
namespace EW
{

    public enum PlatformT { Unknown,Android,Ios,Windows,Linux,OSX}
    public static class Platform
    {

        public static PlatformT CurrentPlatform
        {
            get
            {
                return currentPlatform.Value;
            }
        }

        static Lazy<PlatformT> currentPlatform = Exts.Lazy(GetCurrentPlatform);

        static PlatformT GetCurrentPlatform()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return PlatformT.Windows;
            return PlatformT.Android;
        }
        private static readonly string RootDirectory = "Content";
        public static string GameDir
        {
            get
            {
                //var dir = AppDomain.CurrentDomain.BaseDirectory;
                //var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var dir = "Content";
                if (!dir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    dir += Path.DirectorySeparatorChar;
                Console.WriteLine("platform t:" + Environment.OSVersion.Platform);
                return dir;
            }
        }

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
            //Java.IO.File fieDir =  Android.App.Application.Context.FilesDir;
            //if (File.Exists(fieDir.AbsolutePath))
            //{
            //    System.Diagnostics.Debug.Print(fieDir.Path);
            //}
            //string[] list = Android.App.Application.Context.Assets.List("Content/mods/cnc");
            //var directories = Directory.GetDirectories(fieDir.Path);
            //var dirContent = directories[2];
            //directories = Directory.GetDirectories(dirContent);
            //if (File.Exists(Path.Combine(dirContent, "settings.yaml")))
            //{
            //    System.Diagnostics.Debug.Print(Path.Combine(dirContent,"settings.yaml"));
            //}
            //var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            //dir = Path.Combine(dir, RootDirectory);

            //if (!Directory.Exists(Path.Combine(dir,"mods/cnc")))
            //    Directory.CreateDirectory(Path.Combine(dir, "mods/cnc"));

            //return dir;//+ Path.DirectorySeparatorChar;


            //return Android.App.Application.Context.Assets.List("").FirstOrDefault(x=>x == RootDirectory);

            var localSupportDir = Path.Combine(GameDir, "Support");
            if (Directory.Exists(localSupportDir))
                return localSupportDir + Path.DirectorySeparatorChar;

            var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            switch (CurrentPlatform)
            {
                case PlatformT.Windows:
                    break;
                case PlatformT.OSX:
                    break;
                default:
                    return dir = string.Empty;
            }

            return dir + Path.DirectorySeparatorChar;
        }
        

        public static string ResolvePath(params string[] path)
        {
            return ResolvePath(path.Aggregate(Path.Combine));
        }

        public static string ResolvePath(string path)
        {
            path = path.TrimEnd(new char[] { ' ', '\t' });

            if (path.StartsWith("^", StringComparison.Ordinal))
            {
                path = SupportDir + path.Substring(1);
            }


            //Paths starting with . are relative to the game dir
            if(path == ".")
            {
                return "Content";
            }
            //if (Path.IsPathRooted(path))
            //    throw new ArgumentException("Invalid filename");


            //var uri = new Uri("file:///" + path);
            //path = uri.LocalPath;
            //path = path.Substring(1);
            //path = path.Replace(Path.DirectorySeparatorChar == '\\' ? '/' : '\\', Path.DirectorySeparatorChar);
            //if (path == ".")
            //    return GameDir;

            if (path.StartsWith("./", StringComparison.Ordinal) || path.StartsWith(".\\", StringComparison.Ordinal))
                path = GameDir + path.Substring(2);

            return path;
        }

    }
}