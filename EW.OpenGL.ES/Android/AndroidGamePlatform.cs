using System;
using Android.Content.Res;
namespace EW.OpenGLES.Mobile
{
    /// <summary>
    /// Android 游戏平台
    /// </summary>
	class AndroidGamePlatform:GamePlatform
	{
		public AndroidGamePlatform(Game game):base(game)
		{

            Game.Activity.Game = Game;
            AndroidGameActivity.Paused += Activity_Paused;
            AndroidGameActivity.Resumed += Activity_Resumed;

            _gameWindow = new AndroidGameWindow(Game.Activity, game);
            Window = _gameWindow;

		}
        private bool _initialized;
        private AndroidGameWindow _gameWindow;
        public static bool IsPlayingVdeo { get; set; }

        public override GameRunBehaviour DefaultRunBehaviour
        {
            get
            {
               return GameRunBehaviour.Asynchronous;
            }
        }

        public override void RunLoop()
        {
            throw new NotImplementedException("The Android platform does not support synchronous run loops");
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StartRunLoop()
        {
            _gameWindow.GameView.Resume();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public override bool BeforeUpdate(GameTime gameTime)
        {
            if (!_initialized)
            {
                Game.DoInitialize();
                _initialized = true;
            }
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {

            return !IsPlayingVdeo;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void BeforeInitialize()
        {

            var currentOrientation = AndroidCompatibility.GetAbsoluteOrientation();
            switch (Game.Activity.Resources.Configuration.Orientation)
            {
                case Orientation.Portrait:
                    this._gameWindow.SetOrientation(currentOrientation == DisplayOrientation.PortraitDown ? DisplayOrientation.PortraitDown : DisplayOrientation.Portrait,false);
                    break;
                default:
                    this._gameWindow.SetOrientation(currentOrientation == DisplayOrientation.LandscapeRight ? DisplayOrientation.LandscapeRight : DisplayOrientation.LandscapeLeft, false);
                    break;
            }

            base.BeforeInitialize();
            _gameWindow.GameView.TouchEnabled = true;
        }

        public override bool BeforeRun()
        {
            //Run it as fast as we can to allow for more response on threaded GPU resource creation.
            _gameWindow.GameView.Run();
            return false;
        }

        /// <summary>
        /// 进入全屏模式
        /// </summary>
        public override void EnterFullScreen()
        {
        }

        /// <summary>
        /// 退出全屏模式
        /// </summary>
        public override void ExitFullScreen()
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            //Force the Viewport to be correctly set.
            Game.graphicsDeviceManager.ResetClientBounds();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Activity_Resumed(object sender,EventArgs e)
        {
            if (!IsActive)
            {
                IsActive = true;
                _gameWindow.GameView.Resume();


                if (!_gameWindow.GameView.IsFocused)
                    _gameWindow.GameView.RequestFocus();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Activity_Paused(object sender,EventArgs e)
        {
            if (IsActive)
            {
                IsActive = false;
                _gameWindow.GameView.Pause();
                _gameWindow.GameView.ClearFocus();
                
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override void Present()
        {
            try
            {
                //var device = Game.GraphicsDevice;
                //if (device != null)
                //    device.Present();

                _gameWindow.GameView.SwapBuffers();
            }
            catch(Exception ex)
            {
                Android.Util.Log.Error("Error in swap buffers", ex.ToString());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override void Exit()
        {
            Game.Activity.MoveTaskToBack(true);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AndroidGameActivity.Paused -= Activity_Paused;
                AndroidGameActivity.Resumed -= Activity_Resumed;
            }
            base.Dispose(disposing);
        }

        public override void Log(string Message)
        {
#if LOGGING
            Android.Util.Log.Debug("RAGameDebug",Message);
#endif
        }

    }
}
