using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
namespace EW
{

    public class WarGame:EW.Xna.Platforms.Game
    {

        public static ModData ModData;

        public static InstalledMods Mods { get; private set; }



        GraphicsDeviceManager gdm;
        public WarGame() {
            gdm = new GraphicsDeviceManager(this);
            gdm.IsFullScreen = true;
            gdm.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            Initialize(new Arguments());
        }

        internal static void Initialize(Arguments args)
        {
            string customModPath = null;

            Mods = new InstalledMods(customModPath);
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