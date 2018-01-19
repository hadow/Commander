using System;
using System.Collections.Generic;
using System.Diagnostics;
using EW.Framework.Mobile;
using EW.Framework.Graphics;
using EW.Framework.Touch;
namespace EW.Framework
{
    public partial class Game:IDisposable
    {

        internal GamePlatform Platform;

#if ANDROID
        [CLSCompliant(false)]
        public static AndroidGameActivity Activity { get; internal set; }
#endif
        private bool _isDisposed;
        private static Game _instance = null;

        private TimeSpan _accumulatedElapsedTime;

        private readonly GameTime _gameTime = new GameTime();

        private Stopwatch _gameTimer;
        //public long RunTime
        //{
        //    get { return _gameTimer.ElapsedMilliseconds; }
        //}
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


        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667/2);//60 fps

        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
        
        private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02f);
        

        internal static Game Instance
        {
            get
            {
                return Game._instance;
            }
        }

        private GameServiceContainer _services;
        public GameServiceContainer Services
        {
            get
            {
                return _services;
            }
        }


        private IGraphicsDeviceManager _graphicsDeviceManager;
        private IGraphicsDeviceService _graphicsDeviceService;

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

        #region Events

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> Exiting;

        #endregion


        internal GraphicsDeviceManager graphicsDeviceManager
        {
            get
            {
                if(_graphicsDeviceManager == null)
                {
                    _graphicsDeviceManager = (IGraphicsDeviceManager)Services.GetService(typeof(IGraphicsDeviceManager));
                }
                return (GraphicsDeviceManager)_graphicsDeviceManager;
            }
            set
            {
                if (_graphicsDeviceManager != null)
                    throw new InvalidOperationException("GraphicsDeviceManager already registered for this Game object");
                _graphicsDeviceManager = value;
            }
        }


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


        
        public GameWindow Window
        {
            get
            {
                return Platform.Window;
            }
        }
        
        
        public void RunOneFrame()
        {
            if (Platform == null)
                return;
            if (!Platform.BeforeRun())
                return;

            if (!_initialized)
            {
                DoInitialize();
                _gameTimer = Stopwatch.StartNew();
                _initialized = true;
            }

            BeginRun();

            Tick();

            EndRun();
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
            if (!Platform.BeforeRun())
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
                case GameRunBehaviour.Asynchronous:

                    Platform.AsyncRunLoopEnded += Platform_AsyncRunLoopEnded;
                    Platform.StartRunLoop();
                    break;
                case GameRunBehaviour.Synchronous:
                    DoUpdate(new GameTime());

                    Platform.RunLoop();
                    EndRun();
                    DoExiting();
                    break;
                default:
                    throw new ArgumentException(string.Format("Handling for the fun behaviour {0} is not implemented.", runBehaviour));
            }

        }

        private void Platform_AsyncRunLoopEnded(object sender,EventArgs e)
        {
            AssertNotDisposed();

            var platform = (GamePlatform)sender;
            platform.AsyncRunLoopEnded -= Platform_AsyncRunLoopEnded;
            EndRun();
            DoExiting();
        }

        protected virtual void BeginRun() { }

        protected virtual void EndRun() { }
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

        protected virtual void OnExiting(object sender,EventArgs args)
        {

        }

        public void Exit()
        {
            _shouldExit = true;
            _suppressDraw = true;
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
        /// 初始化
        /// </summary>
        protected virtual void Initialize()
        {
            ApplyChanges(graphicsDeviceManager);


            _graphicsDeviceService = (IGraphicsDeviceService)Services.GetService(typeof(IGraphicsDeviceService));

            if (_graphicsDeviceService != null && _graphicsDeviceService.GraphicsDevice != null)
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

        
        
        [System.Diagnostics.DebuggerNonUserCode]
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
            //Advance the accumulated elapsed time.
            //提前累积经过时间
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks-_previousTicks);
            _previousTicks = currentTicks;

            //If we're in the fixed timestep mode and not enough time has elapsed
            //to perform an update we sleep off the remaining time to save battery lift and/or release CPU time to other threads and processes.
            //如果我们当前处于固定时间步长模式，并且没有足够的时间来执行Update,我们将休息剩下的时间，以节省电量或释放CPU时间到其它线程和进程
            if(IsFixedTimeStep && _accumulatedElapsedTime < TargetElapsedTime)
            {
                var sleepTime = (int)(TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;

                //Note:While sleep can be inaccurate in general it is accurate enough for frame limiting purposes if some fluctuation is an acceptable result.
                //虽然睡眠通常可能不准确，但如果某些波动是可接受的结果，则对于限制帧数来说它又是足够准确的
                System.Threading.Thread.Sleep(sleepTime);
                goto RetryTick;
            }

            //Do not allow any update to take longer than our maximum.
            //不要让任何更新所花费时间比最大值更长
            if(_accumulatedElapsedTime > _maxElapsedTime)
            {
                _accumulatedElapsedTime = _maxElapsedTime;
            }

            if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = TargetElapsedTime;
                var stepCount = 0;

                //Perform as many full fixed length time steps as we can.
                while(_accumulatedElapsedTime >= TargetElapsedTime && !_shouldExit)
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
                    //If we lag more than 5 frames,start thinking we are running slowly
                    //
                    _gameTime.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw,then we are not running slowly,so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                //Draw needs to know the total elapsed time that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {

                //Perform a single variable length update.
                _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
                _gameTime.TotalGameTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;
                DoUpdate(_gameTime);
            }

            //Draw unless the update suppressed it.
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


                //The TouchPanel needs to know the time for when touches arrive.
                TouchPanelState.CurrentTimestamp = gameTime.TotalGameTime;

            }
        }
        
        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();

            if(Platform.BeforeDraw(gameTime) && BeginDraw())
            {
                Draw(gameTime);
                //EndDraw();
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        internal void DoExiting()
        {
            OnExiting(this, EventArgs.Empty);
            UnloadContent();
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
        /// 绘制
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void Draw(GameTime gameTime)
        {
        }


        /// <summary>
        /// 更新
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