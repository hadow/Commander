using System;
using System.Collections.Generic;
using Android.Opengl;
using Android.App;
using Android.OS;
using Android.Content;
namespace EW.Framework
{
    public class GameActivity:Activity
    {
        private GLSurfaceView mGlSurfaceView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            mGlSurfaceView = new GLSurfaceView(this);
            ActivityManager am = (ActivityManager)GetSystemService(Context.ActivityService);
            
            mGlSurfaceView.SetEGLContextClientVersion(2);
            mGlSurfaceView.SetEGLConfigChooser(true);
            var gr = new GameRenderer();
            
            mGlSurfaceView.SetRenderer(gr);
            SetContentView(mGlSurfaceView);
            
        }


        protected override void OnPause()
        {
            base.OnPause();
            mGlSurfaceView.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            mGlSurfaceView.OnResume();
        }


    }
}
