using System;
using System.Collections.Generic;
using System.Diagnostics;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms.Input.Touch;
using EW.Xna.Platforms.Content;
namespace EW.Xna.Platforms
{
    public partial class Game:IDisposable
    {

        private GameComponentCollection _components;

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
        public long RunTime
        {
            get { return _gameTimer.ElapsedMilliseconds; }
        }
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

        public GameComponentCollection Components
        {
            get { return _components; }
        }

        internal static Game Instance
        {
            get
            {
                return Game._instance;
            }
        }

        private ContentManager _content;


        /// <summary>
        /// 绘制组件
        /// </summary>
        private SortingFilteringCollection<IDrawable> _drawables = new SortingFilteringCollection<IDrawable>(
                d => d.Visible,
                (d, handler) => d.VisibleChanged += handler,
                (d,handler)=>d.VisibleChanged-=handler,
                (d1,d2)=>Comparer<int>.Default.Compare(d1.DrawOrder,d2.DrawOrder),
                (d,handler) =>d.DrawOrderChanged+=handler,
                (d,handler) =>d.DrawOrderChanged-=handler
            
            );

        /// <summary>
        /// 逻辑组件
        /// </summary>
        private SortingFilteringCollection<IUpdateable> _updateables = new SortingFilteringCollection<IUpdateable>(
                u => u.Enabled,
                (u, handler) => u.EnabledChanged += handler,
                (u, handler) => u.EnabledChanged -= handler,
                (u1, u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder),
                (u, handler) => u.UpdateOrderChanged += handler,
                (u, handler) => u.UpdateOrderChanged -= handler);


        public Game()
        {
            _instance = this;
            _services = new GameServiceContainer();
            _components = new GameComponentCollection();
            _content = new ContentManager(_services);
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


        public ContentManager Content
        {
            get { return _content; }
            set
            {
                if (value == null)
                    throw new ArgumentException();

                _content = value;
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

            InitializeExistingComponents();

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


            CategorizeComponents();
            _components.ComponentAdded += Components_ComponentAdded;
            _components.ComponentRemoved += Components_ComponentRemoved;

        }

        #region Event Handlers

        private void Components_ComponentAdded(object sender,GameComponentCollectionEventArgs e)
        {
            e.GameComponent.Initialize();
            CategorizeComponent(e.GameComponent);
        }

        private void Components_ComponentRemoved(object sender,GameComponentCollectionEventArgs e)
        {
            DecategorizeComponent(e.GameComponent);
        }

        #endregion


        /// <summary>
        /// 初始化已存在的组件
        /// Note:InitializeExistingComponents really should only be called once. Game.Initialize is the only method in a position to gurantee
        /// that no component will get a duplicate Initialize call.
        /// Further calls to Initialize occur immediately in response to Components.ComponentAdded.
        /// </summary>
        private void InitializeExistingComponents()
        {

            for(int i = 0; i < Components.Count; i++)
            {
                Components[i].Initialize();
            }

        }

        private void CategorizeComponents()
        {
            DecategorizeComponents();
            for(int i = 0; i < Components.Count; i++)
            {
                CategorizeComponent(Components[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        private void CategorizeComponent(IGameComponent component)
        {
            if (component is IUpdateable)
                _updateables.Add((IUpdateable)component);

            if (component is IDrawable)
                _drawables.Add((IDrawable)component);
        }

        private void DecategorizeComponent(IGameComponent component)
        {
            if (component is IUpdateable)
                _updateables.Remove((IUpdateable)component);
            if (component is IDrawable)
                _drawables.Remove((IDrawable)component);
        }

        /// <summary>
        /// 解除连接组件
        /// </summary>
        private void DecategorizeComponents()
        {
            _updateables.Clear();
            _drawables.Clear();
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
                EndDraw();
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
            _drawables.ForEachFilteredItem(DrawAction, gameTime);
        }

        private static readonly Action<IDrawable, GameTime> DrawAction = (drawable, gameTime) => drawable.Draw(gameTime);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void Update(GameTime gameTime)
        {
            _updateables.ForEachFilteredItem(UpdateAction, gameTime);
        }

        public static readonly Action<IUpdateable, GameTime> UpdateAction = (updateable, gametime) => updateable.Update(gametime);

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

                    if(_content != null)
                    {
                        _content.Dispose();
                        _content = null;
                    }
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