using System;
using System.Collections.Generic;
using RA.Mobile.Platforms;

namespace RA.Game
{
    public class WarGame:RA.Mobile.Platforms.Game
    {

        GraphicsDeviceManager gdm;
        public WarGame() {
            gdm = new GraphicsDeviceManager(this);
            gdm.IsFullScreen = true;
            gdm.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
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