using System;
using System.Collections.Generic;
using System.IO;
using EW.Xna.Platforms;
namespace EW
{

    public class WarGame:EW.Xna.Platforms.Game
    {

        public static ModData ModData;
        public static Settings Settings;
        public static InstalledMods Mods { get; private set; }



        GraphicsDeviceManager gdm;
        public WarGame() {
            gdm = new GraphicsDeviceManager(this);
            gdm.IsFullScreen = true;
            gdm.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            Initialize(new Arguments());
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        /// <param name="args"></param>
        public static void InitializeSettings(Arguments args)
        {
            Settings = new Settings(Platform.ResolvePath(Path.Combine("^","settings.yaml")), args);
        }

        internal static void Initialize(Arguments args)
        {
            string customModPath = null;

            InitializeSettings(args);

            customModPath = Android.App.Application.Context.FilesDir.Path;
            Mods = new InstalledMods(customModPath);

            InitializeMod(Settings.Game.Mod, args);
        }

        /// <summary>
        /// 初始化Mod
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="args"></param>
        public static void InitializeMod(string mod,Arguments args)
        {
            if (ModData != null)
            {

                ModData = null;
            }

            ModData = new ModData(Mods[mod], Mods, true);

            using (new Support.PerfTimer("LoadMaps"))
                ModData.MapCache.LoadMaps();
        }

        public static T CreateObject<T>(string name)
        {
            return ModData.ObjectCreator.CreateObject<T>(name);
        }


        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

        }
    }
}