using System;
using System.Diagnostics;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms.Input.Touch;
namespace EW.Xna.Platforms
{
    public class Game:IDisposable
    {

#if ANDROID
        [CLSCompliant(false)]
        public static AndroidGameActivity Activity { get; internal set; }
#endif
        private bool _isDisposed;
        private static Game _instance = null;

        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();
        private Stopwatch _gameTimer;
        private long _previousTicks = 0;
        private int _updateFrameLag;

        private bool _shouldExit;
        private bool _suppressDraw;
        private bool _initialized = false;
        private bool _isFixedTimeStep = true;
        public bool IsFixedTimeStep
        {
            get { return _isFixedTimeStep; }
            set { _isFixedTimeStep = value; }
        }

        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667);//60 fps

        public TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {
                value = Platform.TargetElapsedTimeChanging(value);
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("The time must be positive and non-zero,", default(Exception));
                }
                if(value != _targetElapsedTime)
                {
                    _targetElapsedTime = value;
                    Platform.TargetElapsedTimeChanged();
                }
            }
        }

        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);




        private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02f);
        internal static Game Instance
        {
            get
            {
                return Game._instance;
            }
        }

        public Game()
        {
            _instance = this;
            _services = new GameServiceContainer();

            Platform = GamePlatform.PlatformCreate(this);           //创建移动平台
            Platform.Activated += OnActivated;
            Platform.Deactivated += OnDeactivated;
            _services.AddService(typeof(GamePlatform), Platform);
        }

        ~Game()
        {
            Dispose(false);
        }

        
        internal GamePlatform Platform;
        private GameServiceContainer _services;
        public GameServiceContainer Services
        {
            get
            {
                return _services;
            }
        }
        private IGraphicsDeviceService _graphicsDeviceService;

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if(_graphicsDeviceService == null)
                {
                    _graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));
                    if (_graphicsDeviceService == null)
                        throw new InvalidOperationException("No Graphics Device Service");

                }
                return _graphicsDeviceService.GraphicsDevice;
            }
        }


        private IGraphicsDeviceManager _graphicsDeviceManager;

        internal GraphicsDeviceManager graphicsDeviceManager
        {
            get
            {
                if(_graphicsDeviceManager == null)
                {
                    _graphicsDeviceManager = (IGraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));
                    //if (_graphicsDeviceManager == null)
                    //    throw new InvalidOperationException("No Graphics Device Manager");
                }
                return (GraphicsDeviceManager)_graphicsDeviceManager;
            }
        }
        public GameWindow Window
        {
            get
            {
                return Platform.Window;
            }
        }


        /// <summary>
        /// Running
        /// </summary>
        public void Run()
        {
            Run(Platform.DefaultRunBehaviour);
        }

        public void Run(GameRunBehaviour runBehaviour)
        {
            AssertNotDisposed();
            if(!Platform.BeforeRun())
            {
                BeginRun();
                _gameTimer = Stopwatch.StartNew();
                return;
            }
            if (!_initialized)
            {
                DoInitialize();
                _initialized = true;
            }

            BeginRun();
            _gameTimer = Stopwatch.StartNew();
            switch (runBehaviour)
            {

            }

        }

        protected virtual void BeginRun() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sneder"></param>
        /// <param name="args"></param>
        protected virtual void OnActivated(object sneder,EventArgs args)
        {

        }

        protected virtual void OnDeactivated(object sender,EventArgs args)
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        internal void ApplyChanges(GraphicsDeviceManager manager)
        {
            Platform.BeginScreenDeviceChange(GraphicsDevice.PresentationParameters.IsFullScreen);
            if (GraphicsDevice.PresentationParameters.IsFullScreen)
                Platform.EnterFullScreen();
            else
                Platform.ExitFullScreen();

            var viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            GraphicsDevice.Viewport = viewport;
            Platform.EndScreenDeviceChange(string.Empty, viewport.Width, viewport.Height);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Initialize()
        {
            ApplyChanges(graphicsDeviceManager);

            _graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));

            if(_graphicsDeviceService !=null && _graphicsDeviceService.GraphicsDevice != null)
            {
                LoadContent();
            }
        }



        /// <summary>
        /// 执行初始化操作
        /// </summary>
        internal void DoInitialize()
        {
            AssertNotDisposed();

            if (GraphicsDevice == null && graphicsDeviceManager != null)
                _graphicsDeviceManager.CreateDevice();

            Platform.BeforeInitialize();
            Initialize();
        }
        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                string name = GetType().Name;
                throw new ObjectDisposedException(name, string.Format("The {0} object was used after being Disposed.", name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Tick()
        {
            RetryTick:
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks-_previousTicks);
            _previousTicks = currentTicks;
            if(IsFixedTimeStep && _accumulatedElapsedTime < TargetElapsedTime)
            {
                var sleepTime = (int)(TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
                System.Threading.Thread.Sleep(sleepTime);
                goto RetryTick;
            }

            if(_accumulatedElapsedTime > _maxElapsedTime)
            {
                _accumulatedElapsedTime = _maxElapsedTime;
            }

            if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = TargetElapsedTime;
                var stepCount = 0;
                while(_accumulatedElapsedTime >= TargetElapsedTime)
                {
                    _gameTime.TotalGameTime += TargetElapsedTime;
                    _accumulatedElapsedTime -= TargetElapsedTime;
                    ++stepCount;
                    DoUpdate(_gameTime);
                }

                _updateFrameLag += Math.Max(0, stepCount - 1);
                if (_gameTime.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                        _gameTime.IsRunningSlowly = false;
                }
                else if(_updateFrameLag >= 5)
                {
                    _gameTime.IsRunningSlowly = true;
                }

                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {
                _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
                _gameTime.TotalGameTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;
                DoUpdate(_gameTime);
            }

            if (_suppressDraw)
                _suppressDraw = false;
            else
                DoDraw(_gameTime);

            if (_shouldExit)
                Platform.Exit();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        internal void  DoUpdate(GameTime gameTime)
        {
            AssertNotDisposed();
            if (Platform.BeforeUpdate(gameTime))
            {
                Update(gameTime);

                TouchPanelState.CurrentTimestamp = gameTime.TotalGameTime;

            }
        }
        
        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();

            if(Platform.BeforeDraw(gameTime) && BeginDraw())
            {
                Draw(gameTime);
                EndDraw();
            }
        }

        /// <summary>
        /// 开始绘制
        /// </summary>
        /// <returns></returns>
        protected virtual bool BeginDraw() { return true; }

        /// <summary>
        /// 结束绘制
        /// </summary>
        protected virtual void EndDraw()
        {
            Platform.Present();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void Draw(GameTime gameTime) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void Update(GameTime gameTime)
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        protected virtual void LoadContent() { }

        protected virtual void UnloadContent() { }



        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if(_graphicsDeviceManager != null)
                    {
                        (_graphicsDeviceManager as GraphicsDeviceManager).Dispose();
                        _graphicsDeviceManager = null;
                    }

                    if(Platform != null)
                    {
                        Platform.Activated -= OnActivated;
                        Platform.Deactivated -= OnDeactivated;
                        _services.RemoveService(typeof(GamePlatform));

                        Platform.Dispose();
                        Platform = null;
                    }
                }

#if ANDROID
                Activity = null;
#endif
                _isDisposed = true;
                _instance = null;
            }
        }
    }
}