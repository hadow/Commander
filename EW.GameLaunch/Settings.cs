using System;
using System.Collections.Generic;
using System.IO;
using Java.IO;
namespace EW
{


    public class DebugSettings
    {
        public bool SanityCheckUnsyncedCode = false;

        public string UUID = System.Guid.NewGuid().ToString();


        public bool StrictActivityChecking = false;
    }


    public class GraphicsSettings
    {
        public int SheetSize = 2048;
        public int BatchSize = 4096;

        public string Language = "english";
        public string DefaultLanguage = "china";

        public bool PixelDouble;

        /// <summary>
        /// Add a frame rate limiter.
        /// </summary>
        public bool CapFramerate = true;

        /// <summary>
        /// At whick frames per second to cap the framerate.
        /// </summary>
        public int MaxFramerate = 60;
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

        public bool Mute = false;
        public bool CashTicks = true;

        public bool Repeat = false;

        public bool Shuffle = false;
    }
    
    public class Settings
    {
        string settingFile;

        public readonly GraphicsSettings Graphics = new GraphicsSettings();
        public readonly GameSettings Game = new GameSettings();
        public readonly SoundSettings Sound = new SoundSettings();
        public readonly DebugSettings Debug = new DebugSettings();

        public Dictionary<string, object> Sections;

        public Settings(string file,Arguments args)
        {
            settingFile = file;
            Sections = new Dictionary<string, object>()
            {
                {"Game",Game }, { "Sound",Sound },{"Debug",Debug },
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