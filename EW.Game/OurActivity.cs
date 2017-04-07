using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.App;
using Android.Content.PM;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    [Activity(Label ="Red Alert For Mobile",MainLauncher =true,
        Icon ="@drawable/icon",
        AlwaysRetainTaskState =true,LaunchMode =LaunchMode.SingleInstance,
        ScreenOrientation =ScreenOrientation.SensorLandscape,
        ConfigurationChanges =ConfigChanges.Orientation|ConfigChanges.Keyboard|ConfigChanges.KeyboardHidden)]
    public class OurActivity: EW.Mobile.Platforms.AndroidGameActivity
    {


        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
            var g = new WarGame();
            SetContentView(g.Services.GetService<View>());
            g.Run();
        }
    }
}