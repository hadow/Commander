using System;
using EW.Mobile.Platforms.Graphics;
using EW.Mobile.Platforms.Input.Touch;
namespace EW.Mobile.Platforms
{
    /// <summary>
    /// 抽象的一个游戏平台
    /// </summary>
	abstract partial class GamePlatform:IDisposable
	{
        protected TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);
        protected bool _needsToResetElapsedTime = false;
        bool disposed;
        protected bool _alreadyInFullScreenMode = false;
        protected bool _alreadyInWindowedMode = false;

        protected bool IsDisposed { get { return disposed; } }

        public abstract GameRunBehaviour DefaultRunBehaviour { get; }

        #region Events
        public event EventHandler<EventArgs> AsyncRunLoopEnded;
        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        private void Raise<TEventArgs>(EventHandler<TEventArgs> handler,TEventArgs e) where TEventArgs:EventArgs
        {
            if (handler != null)
                handler(this, e);
        }

        protected void RaiseAsyncRunLoopEnded()
        {
            Raise(AsyncRunLoopEnded, EventArgs.Empty);
        }
        #endregion




        public Game Game
        {
            get;private set;
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            internal set
            {
                if(_isActive != value)
                {
                    _isActive = value;
                    Raise(_isActive ? Activated : Deactivated, EventArgs.Empty);
                }
            }
        }

        private GameWindow _window;
        public GameWindow Window
        {
            get { return _window; }
            protected set
            {
                if(_window == null)
                {
                    //TODO:
                    TouchPanel.PrimaryWindow = value;
                }
                _window = value;
            }
        }

		public GamePlatform(Game game)
		{
            if (game == null)
                throw new ArgumentNullException("game");

            Game = game;
		}

        ~GamePlatform()
        {
            Dispose(false);
        }


        #region Methods

        /// <summary>
        /// 
        /// </summary>
        public virtual void BeforeInitialize()
        {
            IsActive = true;
            if(this.Game.GraphicsDevice == null)
            {
                var graphicsDeviceManager = Game.Services.GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;
                graphicsDeviceManager.CreateDevice();
            }
        }


        public virtual bool BeforeRun()
        {
            return true;
        }

        public abstract void Exit();

        public abstract void RunLoop();

        public abstract void StartRunLoop();

        public abstract bool BeforeUpdate(GameTime gameTime);

        public abstract bool BeforeDraw(GameTime gameTime);

        public abstract void EnterFullScreen();

        public abstract void ExitFullScreen();

        public abstract void BeginScreenDeviceChange(bool willBeFullScreen);

        public abstract void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight);

        public virtual void Present() { }

        internal virtual void OnPresentationChanged()
        {

        }

        public virtual TimeSpan TargetElapsedTimeChanging(TimeSpan value)
        {
            return value;
        }

        public virtual void TargetElapsedTimeChanged() { }

        #endregion Methods





        public void Dispose()
		{
            Dispose(true);
            GC.SuppressFinalize(this);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        [System.Diagnostics.Conditional("DEBUG")]
        public virtual void Log(string Message) { }
	}
}
