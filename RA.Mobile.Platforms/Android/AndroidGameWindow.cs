using System;
using System.Drawing;
using Android.Content;
namespace RA.Mobile.Platforms
{
	public class AndroidGameWindow : GameWindow, IDisposable
	{

		private Rectangle _clientBounds;

		private DisplayOrientation _supportedOrientations = DisplayOrientation.Default;
		private DisplayOrientation _currentOrientation;
		public AndroidGameWindow()
		{
		}


        /// <summary>
        /// 初始化上下文
        /// </summary>
        /// <param name="context"></param>
        private void Initialize(Context context)
        {

        }

		internal protected override void SetSupportedOrientations(DisplayOrientation orientations)
		{
			_supportedOrientations = orientations;
		}

		public override IntPtr Handle
		{
			get
			{
				return IntPtr.Zero;
			}
		}
	
		public override DisplayOrientation CurrentOrientation
		{
			get
			{
				return _currentOrientation;
			}
		}


		public override string ScreenDeviceName
		{
			get
			{
				throw new NotImplementedException();
			}
				
		}


		public override Rectangle ClientBounds
		{
			get
			{
				return _clientBounds;
			}
		}

		public override bool AllowUserResizing
		{
			get
			{
				return false;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		protected override void SetTitle(string title)
		{
			throw new NotImplementedException();
		}


		public override void BeginScreenDeviceChange(bool willBeFullScreen)
		{
			throw new NotImplementedException();
		}


		public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{



		}



	}
}
