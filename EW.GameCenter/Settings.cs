using System;
using System.Collections.Generic;
using System.IO;
using Java.IO;
namespace EW
{

    public class GraphicsSettings
    {
        public int SheetSize = 2048;
        public int BatchSize = 8192;

        public string Language = "english";
        public string DefaultLanguage = "china";

        public bool PixelDouble;
    }
    /// <summary>
    /// 
    /// </summary>
    public class GameSettings
    {
        public string Mod = "modchooser";
        public string PreviousMod = "ra";

        public bool AllowZoom = true;

        public bool ShowShellmap = true;

        public bool DrawTargetLine = true;
    }

    public class SoundSettings
    {
        public float SoundVolume = 0.5f;
        public float MusicVolume = 0.5f;
        public float VideoVolume = 0.5f;
    }
    
    public class Settings
    {
        string settingFile;

        public GraphicsSettings Graphics = new GraphicsSettings();
        public GameSettings Game = new GameSettings();
        public SoundSettings Sound = new SoundSettings();
        public Dictionary<string, object> Sections;

        public Settings(string file,Arguments args)
        {
            settingFile = file;
            Sections = new Dictionary<string, object>()
            {
                {"Game",Game }, { "Sound",Sound }
            };

            var err1 = FieldLoader.UnknownFieldAction;
            var err2 = FieldLoader.InvalidValueAction;

            try
            {
                
                var stream = Android.App.Application.Context.Assets.Open("Content/settings.yaml");
                 //if (File.Exists(settingFile))
                {
                    var yaml = MiniYaml.DictFromStream(stream);
                    foreach(var kv in Sections)
                    {
                        if (yaml.ContainsKey(kv.Key))
                            LoadSectionYaml(yaml[kv.Key], kv.Value);
                    }
                }
            }
            finally
            {

            }
        }

        static void LoadSectionYaml(MiniYaml yaml,object section)
        {
            FieldLoader.Load(section, yaml);
        }
    }
}