using System;
using System.Collections.Generic;
using RA.Mobile.Platforms;
namespace RA.Game
{
    public class WarGame:RA.Mobile.Platforms.Game
    {

        public static ModData ModData;


        GraphicsDeviceManager gdm;
        public WarGame() {
            gdm = new GraphicsDeviceManager(this);
            gdm.IsFullScreen = true;
            gdm.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            using (var runtime = new Eluant.LuaRuntime())
            {
                using(var fn = runtime.CreateFunctionFromDelegate(new Func<int, int>(x => x * x)))
                {
                    runtime.Globals["square"] = fn;
                }
                runtime.DoString("print(square(4))").Dispose();
            }



        }



        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

        }
    }
}