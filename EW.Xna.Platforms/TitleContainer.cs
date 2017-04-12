using System;
using System.IO;
using EW.Xna.Platforms.Utilities;
namespace EW.Xna.Platforms
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class TitleContainer
    {
        static internal string Location { get; private set; }
        static TitleContainer()
        {
            Location = string.Empty;
                
        }


        public static Stream OpenStream(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (Path.IsPathRooted(name))//不接受绝对路径
                throw new ArgumentException("Invalid filename.TitleContainer.OpenStream required a relative path.", name);

            var safeName = NormalizeRelativePath(name);

            Stream stream;
            try
            {
                stream = PlatformOpenStream(safeName);
                if (stream == null)
                    throw new FileNotFoundException("Error loading \""+name+"\".File not found.");

            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new FileNotFoundException(name, ex);
            }
            return stream;


        }

        internal static string NormalizeRelativePath(string name)
        {
            var uri = new Uri("file:///" + name);
            var path = uri.LocalPath;
            path = path.Substring(1);
            return path.Replace(FileHelpers.NotSeparator, FileHelpers.Separator);
        }

    }
}