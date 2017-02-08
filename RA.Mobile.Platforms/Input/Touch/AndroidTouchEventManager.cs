using System;
namespace RA.Mobile.Platforms
{
    /// <summary>
    /// 
    /// </summary>
	public class AndroidTouchEventManager
	{
        readonly AndroidGameWindow _gameWindow;

        public bool Enabled { get; set; }

		public AndroidTouchEventManager(AndroidGameWindow androidGameWindow)
		{
            _gameWindow = androidGameWindow;
		}


	}
}
