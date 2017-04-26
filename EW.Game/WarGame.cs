using System;
using System.Collections.Generic;
using System.IO;
using EW.Xna.Platforms;
using EW.Xna.Platforms.Graphics;
namespace EW
{
    /// <summary>
    /// Our game is derived from the class EW.Xna.Framework.Game 
    /// and is the heart of our application. The Game class is responsible for initializing the graphics device,
    /// loading content and most importantly, running the application game loop. 
    /// The majority of our code is implemented by overriding several of Game’s protected methods.
    /// </summary>
    public class WarGame:EW.Xna.Platforms.Game
    {

        public static ModData ModData;
        public static Settings Settings;
        public static InstalledMods Mods { get; private set; }

        Vector2 position;
        Texture2D texture;
        SpriteBatch spriteBatch;
        GraphicsDeviceManager gdm;
        public WarGame() {
            gdm = new GraphicsDeviceManager(this);
            gdm.IsFullScreen = true;
            gdm.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            position = Vector2.Zero;
            return;
            Initialize(new Arguments());
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
        /// 初始化配置文件
        /// </summary>
        /// <param name="args"></param>
        public static void InitializeSettings(Arguments args)
        {
            Settings = new Settings(Platform.ResolvePath(Path.Combine("^","settings.yaml")), args);
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


        protected override void Initialize()
        {
            texture = new Texture2D(this.GraphicsDevice, 100, 100);
            Color[] colorData = new Color[100 * 100];
            for(int i = 0; i < 10000; i++)
            {
                colorData[i] = Color.Red;
            }

            texture.SetData<Color>(colorData);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }




        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position);
            spriteBatch.End();
            base.Draw(gameTime);

        }

        public static T CreateObject<T>(string name)
        {
            return ModData.ObjectCreator.CreateObject<T>(name);
        }
    }
}