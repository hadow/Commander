using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using EW.Framework;
using EW.Framework.Graphics;
using EW.Support;
using EW.Graphics;
using EW.NetWork;
using EW.Primitives;
using EW.Framework.Touch;
using EW.Widgets;
namespace EW
{
    /// <summary>
    /// Our game is derived from the class EW.Xna.Framework.Game 
    /// and is the heart of our application. The Game class is responsible for initializing the graphics device,
    /// loading content and most importantly, running the application game loop. 
    /// The majority of our code is implemented by overriding several of Game’s protected methods.
    /// </summary>
    public class WarGame:Game
    {
        /// <summary>
        /// 120 ms net tick for 40ms local tick
        /// </summary>
        public const int NetTickScale = 3;
        public const int Timestep = 40;
        public const int TimestepJankThreshold = 250;

        public static MersenneTwister CosmeticRandom = new MersenneTwister();
        public static ModData ModData;
        public static Settings Settings;
        public static InstalledMods Mods { get; private set; }
        public static ExternalMods ExternalMods { get; private set; }
        public static string EngineVersion { get; private set; }
        public GraphicsDeviceManager DeviceManager;


        static Stopwatch stopwatch = Stopwatch.StartNew();

        public static long RunTime
        {
            get
            {
                return stopwatch.ElapsedMilliseconds;
            }
        }
        public static bool IsHost{
            get{
                var id = orderManager.Connection.LocalClientId;
                var client = orderManager.LobbyInfo.ClientWithIndex(id);
                return client != null && client.IsAdmin;
            }
        }
        public static Sound Sound;
        public static Renderer Renderer;
        static WorldRenderer worldRenderer;
        static Server.Server server;
        internal static OrderManager orderManager;
        static volatile ActionQueue delayedActions = new ActionQueue();

        public static event Action BeforeGameStart = () => { };
        public static event Action<OrderManager> ConnectionStateChanged = _ => { };
        static ConnectionState lastConnectionState = ConnectionState.PreConnecting;
        public static int LocalClientId{
            get{
                return orderManager.Connection.LocalClientId;
            }
        }
        public static void RunAfterTick(Action a)
        {
            delayedActions.Add(a, RunTime);
        }

        public static void RunAfterDelay(int delayMilliseconds,Action a)
        {
            delayedActions.Add(a, RunTime + delayMilliseconds);
                
        }


        public static int LocalTick { get { return orderManager.LocalFrameNumber; } }

        public int NetFrameNumber { get { return orderManager.NetFrameNumber; } }


        public static int RenderFrame = 0;


        TouchCollection currentTouchState;

        bool _pinching = false;
        float _pinchInitialDistance;

        public static event Action LobbyInfoChanged = () => { };

        static Task discoverNat;
        public WarGame() {

            //IsFixedTimeStep = false;
            DeviceManager = new GraphicsDeviceManager(this);
            //DeviceManager.DeviceCreated += (object sender, EventArgs args) => {

            //    Initialize(new Arguments());
            //};
            DeviceManager.IsFullScreen = true;
            //DeviceManager.PreferredBackBufferWidth = 960;
            //DeviceManager.PreferredBackBufferHeight = 640;

            DeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        protected override void Initialize()
        {
            TouchPanel.EnabledGestures =GestureType.Tap| GestureType.DoubleTap| GestureType.Pinch | GestureType.PinchComplete | GestureType.FreeDrag | GestureType.DragComplete;
            base.Initialize();

        }

        protected override void LoadContent()
        {
            base.LoadContent();
            Initialize(new Arguments());
        }

        

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="args"></param>
        internal void Initialize(Arguments args)
        {
            string customModPath = null;
            InitializeSettings(args);

            if (Settings.Server.DiscoverNatDevices)
                discoverNat = UPnP.DiscoverNatDevices(Settings.Server.NatDiscoveryTimeout);

            customModPath = Android.App.Application.Context.FilesDir.Path;
            Mods = new InstalledMods(customModPath);

            ExternalMods = new ExternalMods();

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

            if (worldRenderer != null)
                worldRenderer.Dispose();
            worldRenderer = null;

            if (orderManager != null)
                orderManager.Dispose();

            if(ModData != null)
            {
                ModData.ModFiles.UnmountAll();
                ModData.Dispose();
            }

            if (mod == null)
                throw new InvalidOperationException("Game.Mod argument missing.");

            if (!Mods.ContainsKey(mod))
                throw new InvalidOperationException("Unknown or invalid mod '{0}'.".F(mod));

            Renderer = new Renderer(Settings.Graphics,GraphicsDevice);
            Sound = new Sound(Settings.Sound);
            Sound.StopVideo();
            ModData = new ModData(Mods[mod], Mods, true);

            using (new Support.PerfTimer("LoadMaps"))
                ModData.MapCache.LoadMaps();

            ModData.InitializeLoaders(ModData.DefaultFileSystem);
            Renderer.InitializeFont(ModData);
            var grid = ModData.Manifest.Contains<MapGrid>() ? ModData.Manifest.Get<MapGrid>() : null;
            Renderer.InitializeDepthBuffer(grid);


            PerfHistory.Items["render"].HasNormalTick = false;
            PerfHistory.Items["batches"].HasNormalTick = false;
            PerfHistory.Items["render_widgets"].HasNormalTick = false;
            PerfHistory.Items["render_flip"].HasNormalTick = false;

            JoinLocal();

            ModData.LoadScreen.StartGame(args);
        }

        public static void CreateAndStartLocalServer(string mapUID,IEnumerable<Order> setupOrders)
        {
            OrderManager om = null;

            Action lobbyReady = null;

            lobbyReady = () =>
            {
                LobbyInfoChanged -= lobbyReady;
                foreach (var o in setupOrders)
                    om.IssueOrder(o);
            };

            LobbyInfoChanged += lobbyReady;

            om = JoinServer(IPAddress.Loopback.ToString(), CreateLocalServer(mapUID), "");
        }


        public static int CreateLocalServer(string map)
        {
            var settings = new ServerSettings()
            {
                Name = "Skirmish Game",
                Map = map,
                AdvertiseOnline = false
            };

            server = new Server.Server(new IPEndPoint(IPAddress.Loopback, 0), settings, ModData, false);

            return server.Port;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void LoadShellMap()
        {
            var shellmap = ChooseShellMap();
            using (new PerfTimer("StartGame"))
                StartGame(shellmap, WorldT.Shellmap);
        }

        static string ChooseShellMap()
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
        internal static void StartGame(string mapUID,WorldT type)
        {
            //Dispose of the old world before creating a new one.
            if (worldRenderer != null)
                worldRenderer.Dispose();

            BeforeGameStart();
            Map map;
            using (new PerfTimer("PrepareMap"))
                map = ModData.PrepareMap(mapUID);
            using (new PerfTimer("NewWorld"))
                orderManager.World = new World(ModData,map, orderManager, type);

            worldRenderer = new WorldRenderer(ModData, orderManager.World);

            GC.Collect();
            using (new PerfTimer("LoadComplete"))
                orderManager.World.LoadComplete(worldRenderer);
            GC.Collect();
            if (orderManager.GameStarted)
                return;
            UI.FocusWidget = null;

            orderManager.LocalFrameNumber = 0;
            orderManager.LastTickTime = RunTime;
            orderManager.StartGame();
            worldRenderer.RefreshPalette();
            GC.Collect();
        }

        static void JoinLocal(){
            JoinInner(new OrderManager("<no server>",-1,"",new EchoConnection()));
        }

        public static OrderManager JoinServer(string host,int port,string password,bool recordReplay = true)
        {
            var connection = new NetworkConnection(host, port);

            if (recordReplay)
                connection.StartRecording(() => { return TimestampedFilename(); });

            var om = new OrderManager(host, port, password, connection);
            JoinInner(om);
            return om;
        }

        static string TimestampedFilename(bool includemilliseconds = false)
        {
            var format = includemilliseconds ? "yyyy-MM-ddTHHmmssfffZ" : "yyyy-MM-ddTHHmmssZ";
            return "EastWood-" + DateTime.UtcNow.ToString(format, CultureInfo.InvariantCulture);
        }


        static void JoinInner(OrderManager om){

            if (orderManager != null) orderManager.Dispose();
            orderManager = om;
            lastConnectionState = ConnectionState.PreConnecting;
            ConnectionStateChanged(orderManager);
        }

        internal static void SyncLobbyInfo()
        {
            LobbyInfoChanged();
        }

        protected override void Update(GameTime gameTime)
        {
            currentTouchState = TouchPanel.GetState();
            while(TouchPanel.IsGestureAvailable){
               
                var gesture = TouchPanel.ReadGesture();
                UI.HandleInput(gesture);
                switch(gesture.GestureType){
                    case GestureType.Pinch:
                        //Console.WriteLine("pinch:" + Vector2.Distance(gesture.Position,gesture.Position2));
                        float dist = Vector2.Distance(gesture.Position, gesture.Position2);

                        Vector2 aOld = gesture.Position - gesture.Delta;
                        Vector2 bOld = gesture.Position2 - gesture.Delta2;
                        float distOld = Vector2.Distance(aOld, bOld);
                        if(!_pinching)
                        {
                            _pinchInitialDistance = distOld;
                            _pinching = true;
                        }

                        float scale = (distOld - dist) * 0.05f;
                        Console.WriteLine("scale:"+scale +" zoom:"+(dist/_pinchInitialDistance));
                        Zoom(scale);
                        break;
                    case GestureType.PinchComplete:
                        _pinching = false;
                        break;
                    case GestureType.FreeDrag:
                        //worldRenderer.ViewPort.Scroll(gesture.Delta*-1,false);
                        break;

                }

            }
            LogicTick(gameTime);

            //Loop();
        }


        void Zoom(float direction){

            var zoomSteps = worldRenderer.ViewPort.AvailableZoomSteps;
            var currentZoom = worldRenderer.ViewPort.Zoom;
            var nextIndex = zoomSteps.IndexOf(currentZoom);

            if (direction < 0)
                nextIndex--;
            else
                nextIndex++;

            if (nextIndex < 0 || nextIndex >= zoomSteps.Count())
                return;

            var zoom = zoomSteps.ElementAt(nextIndex);
            if (!IsZoomAllowed(zoom))
                return;
            worldRenderer.ViewPort.Zoom = zoom;
        }

        bool IsZoomAllowed(float zoom){

            return zoom >= 1.0f;
        }
        protected override void Draw(GameTime gameTime)
        {
            //Console.WriteLine("gametime:" + gameTime.ElapsedGameTime.TotalMilliseconds);
            RenderTick();
            //GraphicsDevice.Clear(Color.Yellow);
        }

        /// <summary>
        /// 
        /// </summary>
        static void RenderTick()
        {
            using (new PerfSample("render"))
            {
                ++RenderFrame;
                if (worldRenderer != null)
                {
                    Renderer.BeginFrame(worldRenderer.ViewPort.TopLeft, worldRenderer.ViewPort.Zoom);
                    Sound.SetListenerPosition(worldRenderer.ViewPort.CenterPosition.ToVector3());
                    worldRenderer.Draw();

                }
                else
                    Renderer.BeginFrame(Int2.Zero, 1f);

                using(new PerfSample("render_widgets"))
                {
                    Renderer.WorldModelRenderer.BeginFrame();
                    UI.PrepareRenderables();

                    Renderer.WorldModelRenderer.EndFrame();

                    UI.Draw();
                }

                using (new PerfSample("render_flip"))
                    Renderer.EndFrame();
            }


            PerfHistory.Items["render"].Tick();
            PerfHistory.Items["batches"].Tick();
            PerfHistory.Items["render_widgets"].Tick();
            PerfHistory.Items["render_flip"].Tick();
        }

        /// <summary>
        /// 
        /// </summary>
        static void LogicTick(GameTime time=null)
        {
            delayedActions.PerformActions(RunTime);

            if(orderManager.Connection.ConnectionState != lastConnectionState)
            {
                lastConnectionState = orderManager.Connection.ConnectionState;
                ConnectionStateChanged(orderManager);
            }

            InnerLogicTick(orderManager,time);
            if (worldRenderer != null && orderManager.World != worldRenderer.World)
                InnerLogicTick(worldRenderer.World.OrderManager,time);
        }

        /// <summary>
        /// 内部逻辑
        /// </summary>
        /// <param name="orderManager"></param>
        static void InnerLogicTick(OrderManager orderManager,GameTime time)
        {
            //var tick = (long)time.TotalGameTime.TotalMilliseconds;
            var tick = RunTime;

            var world = orderManager.World;

            var uiTickDelta = tick - UI.LastTickTime;

            if(uiTickDelta>Timestep)
            {
                var integralTickTimestep = (uiTickDelta / Timestep) * Timestep;
                UI.LastTickTime += integralTickTimestep >= TimestepJankThreshold ? integralTickTimestep : Timestep;

                Sync.CheckSyncUnchanged(world,UI.Tick);

            }

            var worldTimestep = world == null ? Timestep : world.Timestep;

            var worldTickDelta = tick - orderManager.LastTickTime;

            if(worldTimestep != 0 && worldTickDelta >= worldTimestep)
            {
                using(new PerfSample("tick_time"))
                {
                    //Tick the world to advance the world time to match real time:

                    var integralTickTimestep = (worldTickDelta / worldTimestep) * worldTimestep;
                    orderManager.LastTickTime += integralTickTimestep >= TimestepJankThreshold ? integralTickTimestep : worldTimestep;
                    Sound.Tick();
                    Sync.CheckSyncUnchanged(world,orderManager.TickImmediate);
                    if (world == null) return;

                    var isNetTick = LocalTick % NetTickScale == 0;
                    if (!isNetTick || orderManager.IsReadyForNextFrame)
                    {
                        ++orderManager.LocalFrameNumber;

                        if (isNetTick)
                            orderManager.Tick();


                        Sync.CheckSyncUnchanged(world, () =>
                        {
                            world.OrderGenerator.Tick(world);
                            world.Selection.Tick(world);
                        });

                        world.Tick();
                        PerfHistory.Tick();
                    }
                    else if (orderManager.NetFrameNumber == 0)
                        orderManager.LastTickTime = RunTime;

                    //Wait until we have done our first world Tick before TickRendering

                    if (orderManager.LocalFrameNumber > 0)
                        Sync.CheckSyncUnchanged(world, () => world.TickRender(worldRenderer));
                }
            }
        }

        public static T CreateObject<T>(string name)
        {
            return ModData.ObjectCreator.CreateObject<T>(name);
        }


        /// <summary>
        /// 重启游戏
        /// </summary>
        public static void RestartGame()
        {
            var replay = orderManager.Connection as ReplayConnection;
            var replayName = replay != null ? replay.Filename : null;
            var lobbyInfo = orderManager.LobbyInfo;

            var orders = new[] {
                Order.Command("sync_lobby {0}".F(lobbyInfo.Serialize())),
                Order.Command("startgame")

            };

            Disconnect();
            UI.ResetAll();

            if(replay != null)
            {

            }
            else
            {
                CreateAndStartLocalServer(lobbyInfo.GlobalSettings.Map, orders);
            }
        }

        public static void Disconnect()
        {
            if(orderManager.World != null)
            {

            }
            orderManager.Dispose();
            CloseServer();
            JoinLocal();
        }

        public static void CloseServer()
        {
            if (server != null)
                server.Shutdown();
        }

        //internal static RunStatus CustomRun()
        //{
        //    try
        //    {
        //        Loop();
        //    }
        //    catch(Exception exp)
        //    {
        //        throw exp;
        //    }
        //    finally
        //    {
        //        if (orderManager != null)
        //            orderManager.Dispose();

        //        if (worldRenderer != null)
        //            worldRenderer.Dispose();

        //        ModData.Dispose();
        //        Renderer.Dispose();

        //    }
        //    return state;
        //}


        static RunStatus state = RunStatus.Error;
        static void Loop()
        {
            
            if (state == RunStatus.Running)
                return;

            if (state != RunStatus.Running)
                state = RunStatus.Running;
            const int MaxLogicTicksBehind = 250;

            const int MinReplayFps = 10;

            //Timestamps for when the next logic and rendering should run
            var nextLogic = RunTime;
            var nextRender = RunTime;
            var forcedNextRender = RunTime;

            while(state == RunStatus.Running)
            {
                //Ideal time between logic updates.Timestep = 0 means the game is paused
                //but we still call LogicTick() because it handles pausing internally.
                var logicInterval = worldRenderer != null && worldRenderer.World.Timestep != 0 ? worldRenderer.World.Timestep : Timestep;

                //Ideal time between screen updates.
                var maxFramerate = Settings.Graphics.CapFramerate ? Settings.Graphics.MaxFramerate.Clamp(1, 1000) : 1000;
                var renderInterval = 1000 / maxFramerate;

                var now = RunTime;

                //If the logic has fallen behind to much,skip it and catch up
                if (now - nextLogic > MaxLogicTicksBehind)
                    nextLogic = now;

                var nextUpdate = Math.Min(nextLogic, nextRender);
                if (now >= nextUpdate)
                {
                    var forceRender = now >= forcedNextRender;

                    if(now >= nextLogic)
                    {
                        nextLogic += logicInterval;

                        LogicTick();

                        //Force at least one render per tick during regular gameplay
                        if (orderManager.World != null && !orderManager.World.IsReplay)
                            forceRender = true;
                    }

                    var haveSomeTimeUntilNextLogic = now < nextLogic;
                    var isTimeToRender = now >= nextRender;

                    if ((isTimeToRender && haveSomeTimeUntilNextLogic) || forceRender)
                    {
                        nextRender = now + renderInterval;

                        var maxRenderInterval = Math.Max(1000 / MinReplayFps, renderInterval);
                        forcedNextRender = now + maxRenderInterval;
                        RenderTick();
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep((int)(nextUpdate - now));
                }

            }



        }


        public static Widget LoadWidget(World world,string id,Widget parent,WidgetArgs args){

            return ModData.WidgetLoader.LoadWidget(new WidgetArgs(args){

                {"world",world},
                {"orderManager",orderManager},
                {"worldRenderer",worldRenderer}
            }, parent, id);
        }


        public static bool IsCurrentWorld(World world){

            return orderManager != null && orderManager.World == world && !world.Disposing;
        }
    }
}