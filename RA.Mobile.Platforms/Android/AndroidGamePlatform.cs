using System;
using RA.Mobile.Platforms.Android;
using Android.Content.Res;
namespace RA.Mobile.Platforms
{
	public class AndroidGamePlatform:GamePlatform
	{
		public AndroidGamePlatform(Game game):base(game)
		{
		}
        private bool _initialized;
        private AndroidGameWindow _gameWindow;
        public static bool IsPlayingVdeo { get; set; }

        public override GameRunBehaviour DefaultRunBehaviour
        {
            get
            {
               return GameRunBehaviour.Synchronous;
            }
        }

        public override void RunLoop()
        {
            throw new NotImplementedException("The Android platform does not support synchronous run loops");
        }

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
            _gameWindow.GameView.Run();
            return false;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            Game.graphicsDeviceManager.ResetClientBounds();
        }


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
                var device = Game.GraphicsDevice;
                if (device != null)
                    device.Present();

                _gameWindow.GameView.SwapBuffers();
            }
            catch(Exception exp)
            {

            }
        }


        public override void Exit()
        {
        }

        public override void Log(string Message)
        {
#if LOGGING
            Android.Util.Log.Debug("RAGameDebug",Message);
#endif
        }

    }
}
