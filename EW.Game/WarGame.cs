using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EW.Xna.Platforms;
using EW.Xna.Platforms.Graphics;
using EW.Support;
using EW.Graphics;
using EW.NetWork;
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
        public static MersenneTwister CosmeticRandom = new MersenneTwister();
        public static ModData ModData;
        public static Settings Settings;
        public static InstalledMods Mods { get; private set; }
   
        Vector2 position;
        //Texture2D texture;
        SpriteBatch spriteBatch;
        public GraphicsDeviceManager DeviceManager;

        public Renderer Renderer;
        WorldRenderer worldRenderer;
        OrderManager orderManager;
        public WarGame() {
            DeviceManager = new GraphicsDeviceManager(this);
            DeviceManager.IsFullScreen = true;
            DeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            //Initialize(new Arguments());
        }


        protected override void BeginRun()
        {
            base.BeginRun();
        }

        internal void Initialize(Arguments args)
        {
            string customModPath = null;
            orderManager = new OrderManager();
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
        public void InitializeMod(string mod,Arguments args)
        {
            if (ModData != null)
            {

                ModData = null;
            }


            Renderer = new Renderer();
            ModData = new ModData(GraphicsDevice,Mods[mod], Mods, true);

            using (new Support.PerfTimer("LoadMaps"))
                ModData.MapCache.LoadMaps();

            ModData.InitializeLoaders(ModData.DefaultFileSystem);

            //ModData.LoadScreen.StartGame(args);
            LoadShellMap();
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadShellMap()
        {
            var shellmap = ChooseShellMap();

            using (new PerfTimer("StartGame"))
                StartGame(shellmap, WorldT.Shellmap);
        }

        string ChooseShellMap()
        {
            var shellMaps = ModData.MapCache.Where(m => m.Status == MapStatus.Available && m.Visibility.HasFlag(MapVisibility.Shellmap)).Select(m => m.Uid);

            if (!shellMaps.Any())
                throw new InvalidDataException("No valid shellmaps available");

            return shellMaps.Random(CosmeticRandom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapUID"></param>
        /// <param name="type"></param>
        internal void StartGame(string mapUID,WorldT type)
        {

            if (worldRenderer != null)
                worldRenderer.Dispose();
            Map map;
            using (new PerfTimer("PrepareMap"))
                map = ModData.PrepareMap(mapUID);
            using (new PerfTimer("NewWorld"))
                orderManager.World = new World(map, orderManager, type);

            worldRenderer = new WorldRenderer(ModData, orderManager.World);


                GC.Collect();
        }


        protected override void Initialize()
        {
            //texture = new Texture2D(this.GraphicsDevice, 100, 100);
            //Color[] colorData = new Color[100 * 100];
            //for(int i = 0; i < 10000; i++)
            //{
            //    colorData[i] = Color.Red;
            //}

            //texture.SetData<Color>(colorData);
            base.Initialize();
            Initialize(new Arguments());
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //using (var stream = TitleContainer.OpenStream("Content/charactersheet.png"))
            //{
            //    texture = Texture2D.FromStream(this.GraphicsDevice, stream);
            //}
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

            
            //spriteBatch.Begin();
            //spriteBatch.Draw(texture, position,Color.White);
            //spriteBatch.Draw(texture, new Vector2(100, 100), Color.White);
            //spriteBatch.End();
            base.Draw(gameTime);
            RenderTick();
        }

        /// <summary>
        /// 
        /// </summary>
        void RenderTick()
        {
            using(new PerfSample("render"))
            {
                if (worldRenderer != null)
                {
                    Renderer.BeginFrame(worldRenderer.ViewPort.TopLeft, worldRenderer.ViewPort.Zoom);
                    worldRenderer.Draw();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void LogicTick()
        {

        }

        public static T CreateObject<T>(string name)
        {
            return ModData.ObjectCreator.CreateObject<T>(name);
        }
    }
}