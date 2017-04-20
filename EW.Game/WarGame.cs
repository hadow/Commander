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