using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Util;
using Android.Runtime;
using EW.Framework.Touch;
using Javax.Microedition.Khronos.Egl;
using EW.Framework.Graphics;
using EW.Framework.Input;
namespace EW.Framework.Mobile
{
    [CLSCompliant(false)]//indicates whether a program element is compliant with the Common Language Specification(CLS).This class not be inherited.
    public class AndroidGameView : SurfaceView, ISurfaceHolderCallback, View.IOnTouchListener
    {

        public class BackgroundContext
        {

            EGLContext eglContext;
            AndroidGameView view;
            EGLSurface surface;

            public BackgroundContext(AndroidGameView view)
            {
                this.view = view;
                foreach (var v in EW.Framework.GLESVersion.GetSupportedGLESVersions())
                {
                    eglContext = view.egl.EglCreateContext(view.eglDisplay, view.eglConfig, EGL10.EglNoContext, v.GetAttributes());
                    if (eglContext == null || eglContext == EGL10.EglNoContext)
                    {
                        continue;
                    }
                    break;
                }
                if (eglContext == null || eglContext == EGL10.EglNoContext)
                {
                    eglContext = null;
                    throw new Exception("Could not create EGL context" + view.GetErrorAsString());
                }
                int[] pbufferAttribList = new int[] { EGL10.EglWidth, 64, EGL10.EglHeight, 64, EGL10.EglNone };
                surface = view.CreatePBufferSurface(view.eglConfig, pbufferAttribList);
                if (surface == EGL10.EglNoSurface)
                    throw new Exception("Could not create Pbuffer Surface" + view.GetErrorAsString());
            }

            public void MakeCurrent()
            {
                view.ClearCurrent();
                view.egl.EglMakeCurrent(view.eglDisplay, surface, surface, eglContext);
            }
        }
        internal struct SurfaceConfig
        {
            public int Red;
            public int Green;
            public int Blue;
            public int Alpha;
            public int Depth;
            public int Stencil;

            public int[] ToConfigAttribs()
            {
                List<int> attribs = new List<int>();
                if (Red != 0)
                {
                    attribs.Add(EGL11.EglRedSize);
                    attribs.Add(Red);
                }
                if (Green != 0)
                {
                    attribs.Add(EGL11.EglGreenSize);
                    attribs.Add(Green);
                }
                if (Blue != 0)
                {
                    attribs.Add(EGL11.EglBlueSize);
                    attribs.Add(Blue);
                }
                if (Alpha != 0)
                {
                    attribs.Add(EGL11.EglAlphaSize);
                    attribs.Add(Alpha);
                }
                if (Depth != 0)
                {
                    attribs.Add(EGL11.EglDepthSize);
                    attribs.Add(Depth);
                }
                if (Stencil != 0)
                {
                    attribs.Add(EGL11.EglStencilSize);
                    attribs.Add(Stencil);
                }
                attribs.Add(EGL11.EglRenderableType);
                attribs.Add(4);
                attribs.Add(EGL11.EglNone);

                return attribs.ToArray();
            }

            static int GetAttribute(EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay, int attribute)
            {
                int[] data = new int[1];
                egl.EglGetConfigAttrib(eglDisplay, config, EGL11.EglRedSize, data);
                return data[0];
            }

            public static SurfaceConfig FromEGLConfig(EGLConfig config, IEGL10 egl, EGLDisplay eglDisplay)
            {
                return new SurfaceConfig()
                {
                    Red = GetAttribute(config, egl, eglDisplay, EGL11.EglRedSize),
                    Green = GetAttribute(config, egl, eglDisplay, EGL11.EglGreenSize),
                    Blue = GetAttribute(config, egl, eglDisplay, EGL11.EglBlueSize),
                    Alpha = GetAttribute(config, egl, eglDisplay, EGL11.EglAlphaSize),
                    Depth = GetAttribute(config, egl, eglDisplay, EGL11.EglDepthSize),
                    Stencil = GetAttribute(config, egl, eglDisplay, EGL11.EglStencilSize),
                };
            }

            public override string ToString()
            {
                return string.Format("Red:{0} Green:{1} Blue:{2} Alpha:{3} Depth:{4} Stencil:{5}", Red, Green, Blue, Alpha, Depth, Stencil);
            }
        }

        enum InternalState
        {
            Pausing_UIThread,
            Resuming_UIThread,
            Exiting,

            Paused_GameThread,
            Running_GameThread,
            Exited_GameThread,

            ForceRecreateSurface,   // also used to create the surface the 1st time or when screen orientation changes

        }

        public class FrameEventArgs:EventArgs
        {
            double elapsed;

            /// <summary>
            /// indicates how many seconds of tiem elapsed since the previous event.
            /// </summary>
            public double Time
            {
                get { return elapsed; }
                internal set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException();
                    elapsed = value;
                }
            }
            public FrameEventArgs() { }

            public FrameEventArgs(double elapsed)
            {
                Time = elapsed;
            }

        }    

        bool disposed = false;
        ISurfaceHolder mHolder;
        System.Drawing.Size size;

        object _lockObject = new object();

        //Volatile 关键字表示字段可能被多个并发执行线程修改.声明为volatile 的字段不受编译器优化的限制。 这样可以确保字段在任何时间呈现的都是最新的值
        volatile InternalState _internalState = InternalState.Exited_GameThread;

        bool androidSurfaceAvailable = false;

        bool glSurfaceAvailable;
        bool glContextAvailable;
        bool lostglContext;

        System.Diagnostics.Stopwatch stopWatch;

        double tick = 0;

        bool loaded = false;


        public static event EventHandler OnPauseGameThread;
        public static event EventHandler OnResumeGameThread;

        private readonly AndroidTouchEventManager _touchManager;
        private readonly AndroidGameWindow _gameWindow;
        private readonly Game _game;


        Task renderTask;
        CancellationTokenSource cts = null;

        public bool LogFPS { get; set; }

        public bool RenderOnUIThread { get; set; }

        ManualResetEvent _waitForPausedStateProcessed = new ManualResetEvent(false);
        ManualResetEvent _waitForResumedStateProcessed = new ManualResetEvent(false);
        ManualResetEvent _waitForExitedStateProcessed = new ManualResetEvent(false);

        AutoResetEvent _waitForMainGameLoop = new AutoResetEvent(false);
        AutoResetEvent _workerThreadUIRenderingWait = new AutoResetEvent(false);


        double updates;
        DateTime prevUpdateTime;
        DateTime prevRenderTime;
        DateTime curUpdateTime;
        DateTime curRenderTime;
        FrameEventArgs updateEventArgs = new FrameEventArgs();
        FrameEventArgs renderEventArgs = new FrameEventArgs();

        public delegate void FrameEvent(object sender, FrameEventArgs e);

        public event FrameEvent RenderFrame;
        public event FrameEvent UpdateFrame;


        private IEGL10 egl;
        private EGLDisplay eglDisplay;
        private EGLConfig eglConfig;
        private EGLContext eglContext;
        private EGLSurface eglSurface;

        public bool IsResuming { get; private set; }

        public AndroidGameView(Context context, AndroidGameWindow gameWindow, Game game) : base(context)
        {
            _gameWindow = gameWindow;
            _game = game;
            _touchManager = new AndroidTouchEventManager(gameWindow);
            Init();
        }

        private void Init()
        {
            mHolder = Holder;

            //Add callback to get the SurfaceCreated etc events.
            mHolder.AddCallback(this);
            mHolder.SetType(SurfaceType.Gpu);
        }


        public void SurfaceChanged(ISurfaceHolder holder, global::Android.Graphics.Format Format, int width, int height)
        {
            //Set flag to recreate gl surface or rendering can be bad on orienation change or if app is closed in one orientation and re-opened in another

            lock (_lockObject) {

                // can only be triggered when main loop is running, is unsafe to overwrite other states
                if (_internalState == InternalState.Running_GameThread) {
                    _internalState = InternalState.ForceRecreateSurface;
                }
            }


        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            lock (_lockObject)
                androidSurfaceAvailable = true;

        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            lock (_lockObject)
                androidSurfaceAvailable = false;
        }

        public virtual void MakeCurrent()
        {
            EnsureUndisposed();
            if (!egl.EglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
            {
                Log.Error("AndroidGameView", "Error Make Current" + GetErrorAsString());
            }
        }


        public virtual void ClearCurrent()
        {
            EnsureUndisposed();
            if (!egl.EglMakeCurrent(eglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
            {
                Log.Error("AndroidGameView","Error Clearing Current" + GetErrorAsString());
            }
        }

        public bool TouchEnabled
        {
            get
            {
                return _touchManager.Enabled;
            }
            set
            {
                _touchManager.Enabled = value;
                SetOnTouchListener(value ? this : null);
            }
        }


        public bool OnTouch(View v, MotionEvent e)
        {
            
            _touchManager.OnTouchEvent(e);
            return true;
        }
        

        #region Key and Motion

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {

            if (GamePad.OnKeyUp(keyCode, e))
                return true;
            Keyboard.KeyDown(keyCode);
            if (keyCode == Keycode.Back)
                GamePad.Back = true;


            return true;
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (GamePad.OnKeyUp(keyCode, e))
                return true;
            Keyboard.KeyUp(keyCode);
            return true;
        }


        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (GamePad.OnGenericMotionEvent(e))
                return true;
            return base.OnGenericMotionEvent(e);
        }
        #endregion


        public virtual void SwapBuffers()
        {
            EnsureUndisposed();
            if (!egl.EglSwapBuffers(eglDisplay, eglSurface))
            {
                if(egl.EglGetError() == 0)
                {
                    if (lostglContext)
                        Log.Error("AndroidGameView", "Lost EGL context" + GetErrorAsString());

                    lostglContext = true;
                }
            }
        }

        public virtual void Run()
        {
            Run(0.0);
        }

        public virtual void Run(double updatesPerSecond)
        {
            cts = new CancellationTokenSource();
            if (LogFPS)
            {
                targetFps = currentFps = 0;
                avgFps = 1;
            }
            updates = 1000 / updatesPerSecond;

            var syncContext = SynchronizationContext.Current;

            //We always start a new task,regardless if we render on UI thread or not

            renderTask = Task.Factory.StartNew(() => {

                WorkerThreadFrameDispatcher(syncContext);

            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ContinueWith((t) => {

                OnStopped(EventArgs.Empty);
            });

        }

        protected void WorkerThreadFrameDispatcher(SynchronizationContext uiThreadSyncContext)
        {
            Threading.ResetThread(Thread.CurrentThread.ManagedThreadId);
            try
            {

                stopWatch = System.Diagnostics.Stopwatch.StartNew();
                tick = 0;
                prevUpdateTime = DateTime.Now;
                while (!cts.IsCancellationRequested)
                {
                    // either use UI thread to render one frame or this worker thread
                    bool pauseThread = false;

                    if (RenderOnUIThread) {
                        uiThreadSyncContext.Send((s) => {

                            pauseThread = RunIteration(cts.Token);

                        }, null);
                    }
                    else
                    {
                        pauseThread = RunIteration(cts.Token);
                    }

                    if (pauseThread)
                    {
                        _waitForPausedStateProcessed.Set();
                        _waitForMainGameLoop.WaitOne();//pause this thread.
                    }

                }
            }
            catch(Exception ex)
            {
                Log.Error("AndroidGameView", ex.ToString());
            }
            finally
            {
                bool c = cts.IsCancellationRequested;
                cts = null;

                if (glSurfaceAvailable)
                {
                    DestroyGLSurface();
                }

                if (glContextAvailable)
                {
                    DestroyGLContext();
                    ContextLostInternal();
                }

                lock (_lockObject)
                {
                    _internalState = InternalState.Exited_GameThread;
                }
            }
        }

        // Return true to trigger worker thread pause
        bool RunIteration(CancellationToken token)
        {
            Threading.ResetThread(Thread.CurrentThread.ManagedThreadId);

            InternalState currentState = InternalState.Exited_GameThread;

            lock (_lockObject)
            {
                currentState = _internalState;
            }

            switch (currentState)
            {
                case InternalState.Exiting://when ui thread wants to exit
                    break;
                case InternalState.Exited_GameThread:   //when game thread 
                    break;
                case InternalState.Pausing_UIThread:    //when ui thread wants to pause.
                    processStatePausing();
                    break;
                case InternalState.Paused_GameThread:
                    return true;
                case InternalState.Resuming_UIThread:   //when ui thread wants to resume.

                    processStateResuming();

                    //pause must wait for resume in case pause/resume is called in very quick succession
                    lock (_lockObject)
                    {
                        _waitForResumedStateProcessed.Set();
                    }

                    break;
                case InternalState.Running_GameThread:  //when we are running game
                    processStateRunning(token);

                    break;
                case InternalState.ForceRecreateSurface:
                    processStateForceSurfaceRecreation();
                    break;
                default:
                    processStateDefault();
                    cts.Cancel();
                    break;
            }

            return false;

        }

        public virtual void Pause()
        {
            EnsureUndisposed();


            if (_internalState != InternalState.Running_GameThread)
                return;

            if(RenderOnUIThread == false)
            {
                _waitForResumedStateProcessed.WaitOne();
            }

            _waitForMainGameLoop.Reset();

            bool isAndroidSurfaceAvailable = false;

            lock (_lockObject)
            {
                isAndroidSurfaceAvailable = androidSurfaceAvailable;
                if(!isAndroidSurfaceAvailable)
                {
                    _internalState = InternalState.Paused_GameThread;   // prepare for next game loop iteration.
                }
            }

        }

        protected virtual void OnStopped(EventArgs eventArgs)
        {

        }

        void processStateDefault()
        {
            lock (_lockObject)
            {
                _internalState = InternalState.Exited_GameThread;
            }
        }


        void processStateRunning(CancellationToken token)
        {
            //do not run game if surface is not available
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable)
                    return;
            }

            //check if app wants to exit
            if (token.IsCancellationRequested)
            {
                lock (_lockObject)
                {
                    _internalState = InternalState.Exiting;
                }
                return;
            }


            try
            {
                UpdateAndRenderFrame();
            }
            catch(Exception ex)
            {
                Log.Error("AndroidGameView", "GL Exception occured during RunIteration {0}", ex.Message);
                //throw ex;
            }


            if (updates > 0)
            {
                var t = updates - (stopWatch.Elapsed.TotalMilliseconds - tick);
                if (t > 0)
                {
                    if (LogFPS)
                    {
                        Log.Verbose("AndroidGameView", "took {0:F2}ms,should tak {1:F2}ms, sleeping for {2:F2}", stopWatch.Elapsed.TotalMilliseconds - tick, updates, t);
                    }
                }
            }

        }

        void processStateResuming()
        {

            bool isSurfaceAvailable = false;
            lock(_lockObject)
            {
                isSurfaceAvailable = androidSurfaceAvailable;
            }

            //must sleep outside lock!
            if(!RenderOnUIThread && !isSurfaceAvailable)
            {
                Thread.Sleep(50);
                return;
            }

            //this can happend if pause is triggered immediately after resume so that SurfaceCreated callback doesn't get called yet,
            //in this case we skip the resume process and pause sets a new state.
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable)
                    return;

                //create surface if context is available
                if(glContextAvailable && !lostglContext)
                {
                    try
                    {
                        CreateGLSurface();
                    }
                    catch(Exception ex)
                    {
                        Log.Verbose("AndroidGameView", ex.ToString());
                    }
                }

                //create context if not available
                if((!glContextAvailable || lostglContext))
                {
                    // Start or Restart due to context loss
                    bool contextLost = false;
                    if(lostglContext || glContextAvailable)
                    {
                        //we actually lost the context 
                        //so we need to free up our existing
                        //objects and re-create one.
                        DestroyGLContext();
                        contextLost = true;

                        ContextLostInternal();

                    }

                    CreateGLContext();
                    CreateGLSurface();

                    if(!loaded && glContextAvailable)
                    {
                        OnLoad(EventArgs.Empty);
                    }


                    if(contextLost && glContextAvailable)
                    {
                        //we lost the gl context,we need to let the programmer
                        //know so they can re-create textures etc.
                        ContextSetInternal();
                    }


                }
                else if (glSurfaceAvailable)    //finish state if surface created,may take a frame or two until the android UI thread callbacks fire
                {
                    // trigger callbacks,must resume openAL device here
                    //OnResumeGameThread(this, EventArgs.Empty);

                    //go to next state
                    _internalState = InternalState.Running_GameThread;
                }
            }
        }

        void processStateForceSurfaceRecreation()
        {
            // needed at app start
            lock (_lockObject)
            {
                if (!androidSurfaceAvailable || !glContextAvailable)
                    return;
            }

            DestroyGLSurface();
            CreateGLSurface();

            //go to next state
            lock (_lockObject)
                _internalState = InternalState.Running_GameThread;
        }

        void processStatePausing()
        {
            if (glSurfaceAvailable)
            {
                // Surface we are using needs to go away
                DestroyGLSurface();

                if (loaded)
                    OnUnload(EventArgs.Empty);

            }

            OnPauseGameThread(this, EventArgs.Empty);

            //go to next state
            lock (_lockObject)
            {
                _internalState = InternalState.Paused_GameThread;
            }

        }

        protected void ContextSetInternal()
        {
            if (lostglContext)
            {

                if (_game.GraphicsDevice != null)
                {
                    _game.GraphicsDevice.Initialize();

                    IsResuming = true;
                }
            }

            OnContextSet(EventArgs.Empty);
        }


        protected void CreateGLContext()
        {

            lostglContext = false;

            egl = EGLContext.EGL.JavaCast<IEGL10>();

            eglDisplay = egl.EglGetDisplay(EGL10.EglDefaultDisplay);

            if (eglDisplay == EGL10.EglNoDisplay)
                throw new Exception("Could not get EGL display" + GetErrorAsString());

            int[] version = new int[2];
            if (!egl.EglInitialize(eglDisplay, version))
                throw new Exception("Could not initialize EGL display" + GetErrorAsString());

            int depth = 0;
            int stencil = 0;
            switch (_game.graphicsDeviceManager.PreferredDepthStencilFormat)
            {
                case DepthFormat.Depth16:
                    depth = 16;
                    break;
                case DepthFormat.Depth24:
                    depth = 24;
                    break;
                case DepthFormat.Depth24Stencil8:
                    depth = 24;
                    stencil = 8;
                    break;
                case DepthFormat.None:
                    break;
            }

            List<SurfaceConfig> configs = new List<SurfaceConfig>();
            if (depth > 0)
            {
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = depth, Stencil = stencil });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = depth, Stencil = stencil });
                configs.Add(new SurfaceConfig() { Depth = depth, Stencil = stencil });
                if (depth > 16)
                {
                    configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8, Depth = 16 });
                    configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5, Depth = 16 });
                    configs.Add(new SurfaceConfig() { Depth = 16 });
                }
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            else
            {
                configs.Add(new SurfaceConfig() { Red = 8, Green = 8, Blue = 8, Alpha = 8 });
                configs.Add(new SurfaceConfig() { Red = 5, Green = 6, Blue = 5 });
            }
            configs.Add(new SurfaceConfig() { Red = 4, Green = 4, Blue = 4 });
            int[] numConfigs = new int[1];
            EGLConfig[] results = new EGLConfig[1];

            if (!egl.EglGetConfigs(eglDisplay, null, 0, numConfigs))
            {
                throw new Exception("Could not get config count. " + GetErrorAsString());
            }

            EGLConfig[] cfgs = new EGLConfig[numConfigs[0]];
            egl.EglGetConfigs(eglDisplay, cfgs, numConfigs[0], numConfigs);
            Log.Verbose("AndroidGameView", "Device Supports");
            foreach (var c in cfgs)
            {
                Log.Verbose("AndroidGameView", string.Format(" {0}", SurfaceConfig.FromEGLConfig(c, egl, eglDisplay)));
            }

            bool found = false;
            numConfigs[0] = 0;
            foreach (var config in configs)
            {
                Log.Verbose("AndroidGameView", string.Format("Checking Config : {0}", config));
                found = egl.EglChooseConfig(eglDisplay, config.ToConfigAttribs(), results, 1, numConfigs);
                Log.Verbose("AndroidGameView", "EglChooseConfig returned {0} and {1}", found, numConfigs[0]);
                if (!found || numConfigs[0] <= 0)
                {
                    Log.Verbose("AndroidGameView", "Config not supported");
                    continue;
                }
                Log.Verbose("AndroidGameView", string.Format("Selected Config : {0}", config));
                break;
            }

            if (!found || numConfigs[0] <= 0)
                throw new Exception("No valid EGL configs found" + GetErrorAsString());
            var createdVersion = new EW.Framework.GLESVersion();
            foreach (var v in EW.Framework.GLESVersion.GetSupportedGLESVersions())
            {
                Log.Verbose("AndroidGameView", "Creating GLES {0} Context", v);
                eglContext = egl.EglCreateContext(eglDisplay, results[0], EGL10.EglNoContext, v.GetAttributes());
                if (eglContext == null || eglContext == EGL10.EglNoContext)
                {
                    Log.Verbose("AndroidGameView", string.Format("GLES {0} Not Supported. {1}", v, GetErrorAsString()));
                    eglContext = EGL10.EglNoContext;
                    continue;
                }
                createdVersion = v;
                break;
            }
            if (eglContext == null || eglContext == EGL10.EglNoContext)
            {
                eglContext = null;
                throw new Exception("Could not create EGL context" + GetErrorAsString());
            }
            Log.Verbose("AndroidGameView", "Created GLES {0} Context", createdVersion);
            eglConfig = results[0];
            glContextAvailable = true;
        }

        protected void DestroyGLContext()
        {
            if(eglContext != null)
            {
                if (!egl.EglDestroyContext(eglDisplay, eglContext))
                    throw new Exception("Could not destroy EGL context" + GetErrorAsString());
                eglContext = null;
            }

            if(eglDisplay != null)
            {
                if (!egl.EglTerminate(eglDisplay))
                    throw new Exception("Could not terminate EGL connection" + GetErrorAsString());
                eglDisplay = null;
            }
            glContextAvailable = false;
        }


        protected void CreateGLSurface()
        {
            if (!glSurfaceAvailable)
            {
                try
                {
                    //If there is an existing surface,destroy the old one
                    DestroyGLSurface();

                    eglSurface = egl.EglCreateWindowSurface(eglDisplay, eglConfig, (Java.Lang.Object)this.Holder, null);

                    if (eglSurface == null || eglSurface == EGL10.EglNoSurface)
                        throw new Exception("Could not create EGL window surface"+GetErrorAsString());

                    if (!egl.EglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
                        throw new Exception("Could not make EGL current" + GetErrorAsString());

                    glSurfaceAvailable = true;

                    if (_game.GraphicsDevice != null)
                    {
                        _game.graphicsDeviceManager.ResetClientBounds();
                        
                    }

                    if (EW.Framework.GL.GetError == null)
                        EW.Framework.GL.LoadEntryPoints();
                        
                }
                catch (Exception ex)
                {
                    Log.Error("AndroidGameView", ex.ToString());
                    glSurfaceAvailable = false;
                }
            }
        }


        protected EGLSurface CreatePBufferSurface(EGLConfig config, int[] attribList)
        {
            IEGL10 egl = EGLContext.EGL.JavaCast<IEGL10>();
            EGLSurface result = egl.EglCreatePbufferSurface(eglDisplay, config, attribList);
            if (result == null || result == EGL10.EglNoSurface)
                throw new Exception("EglCreatePBufferSurface");
            return result;
        }

        private string GetErrorAsString()
        {
            switch (egl.EglGetError())
            {
                case EGL10.EglSuccess:
                    return "Success";

                case EGL10.EglNotInitialized:
                    return "Not Initialized";

                case EGL10.EglBadAccess:
                    return "Bad Access";
                case EGL10.EglBadAlloc:
                    return "Bad Allocation";
                case EGL10.EglBadAttribute:
                    return "Bad Attribute";
                case EGL10.EglBadConfig:
                    return "Bad Config";
                case EGL10.EglBadContext:
                    return "Bad Context";
                case EGL10.EglBadCurrentSurface:
                    return "Bad Current Surface";
                case EGL10.EglBadDisplay:
                    return "Bad Display";
                case EGL10.EglBadMatch:
                    return "Bad Match";
                case EGL10.EglBadNativePixmap:
                    return "Bad Native Pixmap";
                case EGL10.EglBadNativeWindow:
                    return "Bad Native Window";
                case EGL10.EglBadParameter:
                    return "Bad Parameter";
                case EGL10.EglBadSurface:
                    return "Bad Surface";

                default:
                    return "Unknown Error";
            }
        }

        protected void DestroyGLSurface()
        {

            if(!(eglSurface == null || eglSurface == EGL10.EglNoSurface))
            {
                if (!egl.EglMakeCurrent(eglDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                {
                    Log.Verbose("AndroidGameView", "Could not unbind EGL surface");
                }

                if(!egl.EglDestroySurface(eglDisplay,eglSurface))
                {
                    Log.Verbose("AndroidGameView", "Could not destroy EGL surface");
                }
            }

            eglSurface = null;
            glSurfaceAvailable = false;

        }

        // this method is called on the main thread
        void UpdateAndRenderFrame()
        {

            curUpdateTime = DateTime.Now;

            if(prevUpdateTime.Ticks != 0)
            {
                var t = (curUpdateTime - prevUpdateTime).TotalMilliseconds;
                updateEventArgs.Time = t < 0 ? 0 : t;

            }

            try
            {
                UpdateFrameInternal(updateEventArgs);
            }
            catch(Exception ex)
            {
                if (RenderOnUIThread)
                    throw ex;
                else
                {
                    Game.Activity.RunOnUiThread(() =>
                    {
                        throw ex;
                    });
                }
            }
            prevUpdateTime = curUpdateTime;

            curRenderTime = DateTime.Now;
            if(prevRenderTime.Ticks != 0)
            {
                var t = (curRenderTime - prevRenderTime).TotalMilliseconds;
                renderEventArgs.Time = t < 0 ? 0 : t;
            }

            RenderFrameInternal(renderEventArgs);

            prevRenderTime = curRenderTime;



        }

        void UpdateFrameInternal(FrameEventArgs e)
        {
            OnUpdateFrame(e);
            if (UpdateFrame != null)
                UpdateFrame(this, e);
        }

        protected virtual void OnUpdateFrame(FrameEventArgs e)
        {

        }

        void RenderFrameInternal(FrameEventArgs e)
        {
            if (LogFPS)
            {
                Mark();
            }
            OnRenderFrame(e);

            if (RenderFrame != null)
                RenderFrame(this, e);
        }

        int frames = 0;
        double prev = 0;
        double avgFps = 0;
        double currentFps = 0;
        double targetFps = 0;
        void Mark()
        {
            double cur = stopWatch.Elapsed.TotalMilliseconds;
            if (cur < 2000)
                return;
            frames++;
            if (cur - prev > 995)
            {
                avgFps = 0.8 * avgFps + 0.2 * frames;
                Log.Verbose("AndroidGameView", "frames {0} elapsed {1} ms {2:F2} fps", frames, cur - prev, avgFps);
                frames = 0;
                prev = cur;
            }
        }

        protected virtual void OnRenderFrame(FrameEventArgs e)
        {

        }

        protected void ContextLostInternal()
        {
            OnContextLost(EventArgs.Empty);
        }


        protected virtual void OnContextLost(EventArgs eventArgs)
        {

        }

        protected virtual void OnUnload(EventArgs eventArgs)
        {

        }

        protected virtual void OnLoad(EventArgs eventArgs)
        {

        }

        protected virtual void OnContextSet(EventArgs eventArgs)
        {

        }

        protected void EnsureUndisposed()
        {
            if (disposed)
                throw new ObjectDisposedException("");
        }

        public virtual void Resume()
        {
            EnsureUndisposed();

            lock (_lockObject)
            {
                _waitForResumedStateProcessed.Reset();
                _internalState = InternalState.Resuming_UIThread;

            }

            _waitForMainGameLoop.Set();

            try
            {
                if (!IsFocused)
                    RequestFocus();
            }
            catch { }

            //do not wait for state transition here since surface creation must be triggered first.
        }


    }
}