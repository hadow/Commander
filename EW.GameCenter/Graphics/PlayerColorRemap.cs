using System;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    public class PlayerColorRemap:IPaletteRemap
    {

        public PlayerColorRemap(int[] ramp,HSLColor c,float rampFraction)
        {

        }


        public Color GetRemappedColor(Color original,int index)
        {
            Color c = new Color();
            return c;
        }


    }
}