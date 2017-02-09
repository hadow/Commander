using System;
using System.ComponentModel;
using RA.Mobile.Platforms.Input.Touch;
namespace RA.Mobile.Platforms
{
	/// <summary>
	/// Game window.
	/// </summary>
	public abstract class GameWindow
	{
		

#region Properties
		[DefaultValue(false)]
		public abstract bool AllowUserResizing { get; set; }

		public abstract Rectangle ClientBounds { get; }


		public abstract DisplayOrientation CurrentOrientation { get; }

		public abstract IntPtr Handle { get; }

		public abstract string ScreenDeviceName { get; }

		private string _title;

        #endregion Properties


        internal TouchPanelState TouchPanelState;
        public GameWindow()
        {
            TouchPanelState = new TouchPanelState(this);
        }
        #region Events
        public event EventHandler<EventArgs> ClientSizeChanged;
		public event EventHandler<EventArgs> OrientationChanged;
		public event EventHandler<EventArgs> ScreenDeviceNameChanged;

		#endregion Events


		public abstract void BeginScreenDeviceChange(bool willBeFullScreen);

		public abstract void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight);



		protected void OnActivated() { }

		internal void OnClientSizeChanged()
		{

			if (ClientSizeChanged != null)
			{
				ClientSizeChanged(this, EventArgs.Empty);
			}
		}

		protected void OnDeactivated() { }


		protected void OnOrientationChanged()
		{
			if (OrientationChanged != null)
			{
				OrientationChanged(this, EventArgs.Empty);
			}
		}

		protected void OnPaint() { }

		protected void OnScreenDeviceNameChanged()
		{
			if (ScreenDeviceNameChanged != null)
			{
				ScreenDeviceNameChanged(this, EventArgs.Empty);
			}
		}

		protected abstract void SetTitle(string title);

		protected internal abstract void SetSupportedOrientations(DisplayOrientation orientations);





	}
}
