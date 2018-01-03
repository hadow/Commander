using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.App;
using Android.Content;
using Android.Content.PM;
namespace EW
{

    public enum RunStatus
    {
        Error = -1,
        Success = 0,
        Running = int.MaxValue,
    }
    /// <summary>
    /// 
    /// </summary>
    /// 
    [Activity(MainLauncher =true,
        Icon ="@drawable/icon",
        AlwaysRetainTaskState =true,LaunchMode =LaunchMode.SingleInstance,
        ScreenOrientation =ScreenOrientation.SensorLandscape,
        ConfigurationChanges =ConfigChanges.Orientation|ConfigChanges.Keyboard|ConfigChanges.KeyboardHidden)]
    //[IntentFilter(new[] { Intent.ActionView},Categories =new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },DataScheme ="file",DataMimeType ="*/*",DataPathPattern =".*\\.md")]
    public class MyActivity: EW.Framework.Mobile.AndroidGameActivity
    {
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);


            //string action = Intent.Action;
            //string type = Intent.Type;
            //if(Intent.ActionView.Equals(action) && !string.IsNullOrEmpty(type))
            //{

            //}
            //Android.Net.Uri fileUri = Intent.Data;
            //string path = fileUri.Path;

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            var wg = new WarGame();
            
            SetContentView(wg.Services.GetService<View>());
            wg.Run();
            
        }
    }
}